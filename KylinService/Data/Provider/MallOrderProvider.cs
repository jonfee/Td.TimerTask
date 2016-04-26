using KylinService.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Provider
{
    public class MallOrderProvider
    {
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static MallOrderModel GetOrder(long orderID)
        {
            using (var db = new DataContext())
            {
                var order = db.Mall_Order.SingleOrDefault(p => p.OrderID == orderID);

                return null != order ? new MallOrderModel
                {
                    ActualOrderAmount = order.ActualOrderAmount,
                    CreateTime = order.CreateTime,
                    NeedPayTime = order.NeedPayTime,
                    OrderCode = order.OrderCode,
                    OrderID = order.OrderID,
                    OrderStatus = order.OrderStatus,
                    OrderType = order.OrderType,
                    ProductInfo = order.ProductInfo,
                    ShipTime = order.ShipTime,
                    SourceDataID = order.SourceDataID,
                    UserID = order.UserID,
                    AreaID=order.AreaID
                } : null;
            }
        }

        /// <summary>
        /// 自动取消订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public async static Task<bool> AutoCancelOrder(long orderID)
        {
            using (var db = new DataContext())
            {
                int realRows = 100;
                int rows = 0;

                //只许成功，不许失败
                while (rows != realRows)
                {
                    var order = db.Mall_Order.SingleOrDefault(p => p.OrderID == orderID);

                    if (order.OrderStatus != (int)B2COrderStatus.WaitingPayment) throw new Exception("订单状态已被更改，本次操作失败！");

                    order.OrderStatus = (int)B2COrderStatus.Canceled;
                    order.CancelTime = DateTime.Now;

                    #region 获取订单中商品（SKU）的购买数，以便退回库存

                    //订单中商品 （SKU）的购买数
                    var productSkuQuery = from p in db.Mall_ProductSKU
                                          join o in db.Mall_OrderProductSnapshot
                                          on p.SkuID equals o.SkuID
                                          where o.OrderID == orderID
                                          select new
                                          {
                                              ProductID = p.ProductID,
                                              SkuID = p.SkuID,
                                              RowVersion = p.RowVersion,
                                              SoldNumber = p.SoldNumber,
                                              Inventory = p.Inventory,
                                              BuyCount = o.BuyCounts
                                          };

                    var skuList = productSkuQuery.ToList();

                    //商品及总数量
                    Dictionary<long, int> proCountDic = new Dictionary<long, int>();

                    foreach (var item in skuList)
                    {
                        //库存加，销量减
                        var sku = new Mall_ProductSKU { SkuID = item.SkuID, SoldNumber = item.SoldNumber - item.BuyCount, Inventory = item.Inventory + item.BuyCount, RowVersion = item.RowVersion };

                        db.Attach(sku);
                        db.Entry(sku).Property(p => p.SoldNumber).IsModified = true;
                        db.Entry(sku).Property(p => p.Inventory).IsModified = true;

                        //记录商品数量
                        if (proCountDic.ContainsKey(item.ProductID))
                        {
                            proCountDic[item.ProductID] += item.BuyCount;
                        }
                        else
                        {
                            proCountDic.Add(item.ProductID, item.BuyCount);
                        }
                    }

                    var productQuery = from p in db.Mall_Product
                                       where proCountDic.Keys.Contains(p.ProductID)
                                       select new
                                       {
                                           ProductID = p.ProductID,
                                           Inventory = p.Inventory,
                                           SoldNumber = p.SoldNumber,
                                           RowVersion = p.RowVersion
                                       };

                    var productList = productQuery.ToList();

                    foreach (var item in productList)
                    {
                        var buyCount = proCountDic[item.ProductID];
                        var pro = new Mall_Product { ProductID = item.ProductID, SoldNumber = item.SoldNumber - buyCount, Inventory = item.Inventory + buyCount, RowVersion = item.RowVersion };

                        db.Attach(pro);
                        db.Entry(pro).Property(p => p.Inventory).IsModified = true;
                        db.Entry(pro).Property(p => p.SoldNumber).IsModified = true;
                    }

                    #endregion

                    realRows = skuList.Count() + proCountDic.Count() + 1;

                    rows = await db.SaveChangesAsync();

                    Thread.Sleep(1000);
                }

                return true;
            }
        }
    }
}

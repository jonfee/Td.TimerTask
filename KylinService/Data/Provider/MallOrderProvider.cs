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
                    AreaID = order.AreaID
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
                //预期影响行数
                int expectRows = 0;
                //实际影响行数
                int actualRows = 0;

                //剩余可执行次数（即失败时最多剩余的重执行次数）
                int remainTimes = 5;

                do
                {
                    var order = db.Mall_Order.SingleOrDefault(p => p.OrderID == orderID);

                    if (null == order) throw new Exception("订单数据不存在！");

                    if (order.OrderStatus != (int)B2COrderStatus.WaitingPayment) throw new Exception("订单状态已被更改，本次操作失败！");

                    //是否为摇一摇订单
                    bool isShakeOrder = order.OrderType == (int)B2COrderType.ShakeOrder;

                    //订单状态变更
                    order.OrderStatus = (int)B2COrderStatus.Canceled;
                    order.CancelTime = DateTime.Now;
                    expectRows++;

                    #region 获取订单中商品（SKU）的购买数，以便退回库存

                    //摇一摇订单处理
                    if (isShakeOrder)
                    {
                        //抽中记录
                        var shakeRecord = db.Shake_UserRecord.SingleOrDefault(p => p.RecordID == order.SourceDataID);

                        if (null != shakeRecord)
                        {
                            //摇一摇数据内容
                            var shakeContent = db.Shake_Content.SingleOrDefault(p => p.ContentID == shakeRecord.ContentID);
                            //返回销量
                            if (null != shakeContent)
                            {
                                shakeContent.ConfirmCount -= 1;
                                expectRows++;
                            }
                        }
                    }
                    //普通订单处理
                    else
                    {
                        //订单中商品 （SKU）的购买数
                        var productSkuQuery = from o in db.Mall_OrderProductSnapshot
                                              where o.OrderID == orderID
                                              select new
                                              {
                                                  ProductID = o.ProductID,
                                                  SkuID = o.SkuID,
                                                  BuyCount = o.BuyCounts
                                              };

                        var skuList = productSkuQuery.ToList();

                        //商品及总数量
                        Dictionary<long, int> proCountDic = new Dictionary<long, int>();

                        foreach (var item in skuList)
                        {
                            //当前SKU
                            var sku = db.Mall_ProductSKU.SingleOrDefault(p => p.SkuID == item.SkuID);
                            // new Mall_ProductSKU { SkuID = item.SkuID, SoldNumber = item.SoldNumber - item.BuyCount, Inventory = item.Inventory + item.BuyCount, RowVersion = item.RowVersion };

                            if (null == sku) continue;

                            db.Mall_ProductSKU.Attach(sku);
                            db.Entry(sku).Property(p => p.SoldNumber).IsModified = true;
                            db.Entry(sku).Property(p => p.Inventory).IsModified = true;
                            //库存加，销量减
                            sku.SoldNumber -= item.BuyCount;
                            sku.Inventory += item.BuyCount;
                            expectRows++;

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

                        foreach (var kv in proCountDic)
                        {
                            var pro = db.Mall_Product.SingleOrDefault(p => p.ProductID == kv.Key);
                            //new Mall_Product { ProductID = item.ProductID, SoldNumber = item.SoldNumber - buyCount, Inventory = item.Inventory + buyCount, RowVersion = item.RowVersion };

                            if (null == pro) continue;

                            db.Mall_Product.Attach(pro);
                            db.Entry(pro).Property(p => p.Inventory).IsModified = true;
                            db.Entry(pro).Property(p => p.SoldNumber).IsModified = true;
                            //商品主记录库存加，销量减
                            pro.Inventory += kv.Value;
                            pro.SoldNumber -= kv.Value;
                            expectRows++;
                        }
                    }

                    #endregion

                    actualRows = await db.SaveChangesAsync();

                    //未达到预期，线程休眠1000毫秒
                    if (actualRows != expectRows)
                    {
                        remainTimes--;
                        Thread.Sleep(1000);
                    }

                } while (actualRows != expectRows && remainTimes > 0);

                return actualRows == expectRows;
            }
        }
    }
}

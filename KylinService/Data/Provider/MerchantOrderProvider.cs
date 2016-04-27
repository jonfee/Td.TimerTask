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
    public class MerchantOrderProvider
    {
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static MerchantOrderModel GetOrder(long orderID)
        {
            using (var db = new DataContext())
            {
                var order = db.Merchant_Order.SingleOrDefault(p => p.OrderID == orderID);

                return null != order ? new MerchantOrderModel
                {
                    ActualOrderAmount = order.ActualOrderAmount,
                    CreateTime = order.CreateTime,
                    OrderCode = order.OrderCode,
                    OrderID = order.OrderID,
                    OrderStatus = order.OrderStatus,
                    UserID = order.UserID,
                    SendTime = order.SendTime,
                    MerchantID = order.MerchantID
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
                    var order = db.Merchant_Order.SingleOrDefault(p => p.OrderID == orderID);

                    if (order.OrderStatus != (int)MerchantOrderStatus.WaitingPayment) throw new Exception("订单状态已被更改，本次操作失败！");

                    order.OrderStatus = (int)MerchantOrderStatus.Canceled;
                    order.CancelTime = DateTime.Now;

                    #region 获取订单中商品的购买数，以便退回库存

                    var productQuery = from p in db.MerchGoods_Goods
                                       join o in db.Merchant_OrderSnapshot
                                       on p.GoodsID equals o.GoodsID
                                       where o.OrderID == orderID
                                       select new
                                       {
                                           GoodsID = p.GoodsID,
                                           Inventory = p.Inventory,
                                           SaleNumber = p.SaleNumber,
                                           BuyCount = o.BuyCounts,
                                           RowVersion = p.RowVersion
                                       };

                    var productList = productQuery.ToList();

                    foreach (var item in productList)
                    {
                        var pro = new MerchGoods_Goods { GoodsID = item.GoodsID, SaleNumber = item.SaleNumber - item.BuyCount, Inventory = item.Inventory + item.BuyCount, RowVersion = item.RowVersion };

                        db.Attach(pro);
                        db.Entry(pro).Property(p => p.Inventory).IsModified = true;
                        db.Entry(pro).Property(p => p.SaleNumber).IsModified = true;
                    }

                    #endregion

                    realRows = productList.Count() + 1;

                    rows = await db.SaveChangesAsync();

                    Thread.Sleep(1000);
                }

                return true;
            }
        }
    }
}

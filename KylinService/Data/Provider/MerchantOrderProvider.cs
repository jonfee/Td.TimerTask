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
                //预期影响行数
                int expectRows = 0;
                //实际影响行数
                int actualRows = 0;

                //剩余可执行次数（即失败时最多剩余的重执行次数）
                int remainTimes = 5;

                do
                {
                    var order = db.Merchant_Order.SingleOrDefault(p => p.OrderID == orderID);

                    if (null == order) throw new Exception("订单数据不存在！");

                    if (order.OrderStatus != (int)MerchantOrderStatus.WaitingPayment) throw new Exception("订单状态已被更改，本次操作失败！");

                    order.OrderStatus = (int)MerchantOrderStatus.Canceled;
                    order.CancelTime = DateTime.Now;
                    expectRows++;

                    #region 获取订单中商品的购买数，以便退回库存

                    var productQuery = from o in db.Merchant_OrderSnapshot
                                       where o.OrderID == orderID
                                       select new
                                       {
                                           GoodsID = o.GoodsID,
                                           BuyCount = o.BuyCounts
                                       };

                    var productList = productQuery.ToList();

                    foreach (var item in productList)
                    {
                        var pro = db.MerchGoods_Goods.SingleOrDefault(p => p.GoodsID == item.GoodsID);
                        //new MerchGoods_Goods { GoodsID = item.GoodsID, SaleNumber = item.SaleNumber - item.BuyCount, Inventory = item.Inventory + item.BuyCount, RowVersion = item.RowVersion };

                        if (null == pro) continue;

                        db.MerchGoods_Goods.Attach(pro);
                        db.Entry(pro).Property(p => p.Inventory).IsModified = true;
                        db.Entry(pro).Property(p => p.SaleNumber).IsModified = true;
                        //库存加，销量减
                        pro.SaleNumber -= item.BuyCount;
                        pro.Inventory += item.BuyCount;
                        expectRows++;
                    }

                    #endregion

                    actualRows = await db.SaveChangesAsync();

                    //未达到预期，线程休眠1000毫秒
                    if (actualRows != expectRows)
                    {
                        remainTimes--;
                        Thread.Sleep(1000);
                    }

                } while (expectRows != actualRows && remainTimes > 0);

                return expectRows == actualRows;
            }
        }
    }
}

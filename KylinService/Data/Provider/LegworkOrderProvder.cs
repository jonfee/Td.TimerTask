using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Provider
{
    public class LegworkOrderProvder
    {
        /// <summary>
        /// 自动清理过期没人报价订单
        /// </summary>
        /// <returns></returns>
        public static int AutomaticProcessingOverdueOrders()
        {
            using (var db = new DataContext())
            {
                var list = db.Legwork_Order.Where(q => q.Status == (int)LegworkOrderStatus.WaitingHandle).Select(t => new
                {
                    t.OrderID,
                    t.SubmitTime,
                });
                var legworkConfig = Td.Kylin.DataCache.CacheCollection.LegworkGlobalConfigCache.Value()?.FirstOrDefault();
                if (legworkConfig == null)
                    legworkConfig = new LegworkGlobalConfigCacheModel();
                foreach (var item in list)
                {
                    var CancelTime = item.SubmitTime.AddSeconds(legworkConfig.OrderTimeout);
                    //订单过期时间
                    TimeSpan Tspan = new TimeSpan(CancelTime.Hour, CancelTime.Minute, 0);
                    //服务器当前时间
                    TimeSpan DateTspan = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

                    if (Tspan >= DateTspan)
                    {
                        var model = db.Legwork_Order.FirstOrDefault(q => q.OrderID == item.OrderID);
                        model.Status = (int)LegworkOrderStatus.Canceled;
                        model.CancelTime = DateTime.Now;
                        db.Legwork_Order.Attach(model);
                    }
                }
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 自动清理过期未支付订单-去送物品
        /// </summary>
        /// <returns></returns>
        public static int AutomaticduePaymentTimeoutOrders()
        {
            using (var db = new DataContext())
            {
                var list = db.Legwork_Order.Where(q => q.Status == (int)LegworkOrderStatus.WaitingPayment && q.OrderType == (int)LegworkOrderType.DeliveryGoods).Select(t => new
                {
                    t.OrderID,
                    t.OfferAcceptTime,
                });
                var legworkConfig = Td.Kylin.DataCache.CacheCollection.LegworkGlobalConfigCache.Value()?.FirstOrDefault();
                if (legworkConfig == null)
                    legworkConfig = new LegworkGlobalConfigCacheModel();
                foreach (var item in list)
                {
                    if (item.OfferAcceptTime.HasValue)
                    {
                        var CancelTime = item.OfferAcceptTime.Value.AddSeconds(legworkConfig.PaymentTimeout);
                        //订单过期时间
                        TimeSpan Tspan = new TimeSpan(CancelTime.Hour, CancelTime.Minute, 0);
                        //服务器当前时间
                        TimeSpan DateTspan = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

                        if (Tspan >= DateTspan)
                        {
                            var model = db.Legwork_Order.FirstOrDefault(q => q.OrderID == item.OrderID);
                            model.Status = (int)LegworkOrderStatus.Invalid;
                            model.CancelTime = DateTime.Now;
                            db.Legwork_Order.Attach(model);
                        }
                    }
                }
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 自动处理收货订单
        /// </summary>
        /// <returns></returns>
        public static int AutomaticProcessingOfReceiptOrder()
        {
            using (var db = new DataContext())
            {
                var list = db.Legwork_Order.Where(q => q.Status == (int)LegworkOrderStatus.Delivered).Select(t => new
                {
                    t.OrderID,
                    t.ActualDeliveryTime,
                });
                var legworkConfig = Td.Kylin.DataCache.CacheCollection.LegworkGlobalConfigCache.Value()?.FirstOrDefault();
                if (legworkConfig == null)
                    legworkConfig = new LegworkGlobalConfigCacheModel();
                foreach (var item in list)
                {

                    if (item.ActualDeliveryTime.HasValue)
                    {
                        var CancelTime = item.ActualDeliveryTime.Value.AddSeconds(legworkConfig.AutoConfirmTime);
                        //订单过期时间
                        TimeSpan Tspan = new TimeSpan(CancelTime.Hour, CancelTime.Minute, 0);
                        //服务器当前时间
                        TimeSpan DateTspan = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

                        if (Tspan >= DateTspan)
                        {
                            var model = db.Legwork_Order.FirstOrDefault(q => q.OrderID == item.OrderID);
                            model.Status = (int)LegworkOrderStatus.Complete;
                            model.CompleteTime = DateTime.Now;
                            db.Legwork_Order.Attach(model);
                        }
                    }
                }
                return db.SaveChanges();
            }
        }
    }
}

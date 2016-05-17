using KylinService.Core;
using KylinService.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Provider
{
    public class AppointOrderProvider
    {
        /// <summary>
        /// 获取上门预约订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static AppointOrderModel GetAppointOrder(long orderID)
        {
            using (var db = new DataContext())
            {
                var query = from o in db.KylinService_Order
                            where o.OrderID == orderID
                            join b in db.KylinService_Business
                            on o.BusinessID equals b.BusinessID
                            select new AppointOrderModel
                            {
                                ActualOrderAmount = o.ActualOrderAmount,
                                PaiedTime = o.PaiedTime,
                                BusinessID = o.BusinessID,
                                BusinessType = o.BusinessType,
                                ConfirmTime = o.ConfirmTime,
                                CreateTime = o.CreateTime,
                                MerchantID = o.MerchantID,
                                OrderCode = o.OrderCode,
                                OrderID = o.OrderID,
                                PaymentType = o.PaymentType,
                                PrepaidAmount = o.PrepaidAmount,
                                ReceivedTime = o.ReceivedTime,
                                ServerType = o.ServerType,
                                Status = o.Status,
                                UserFinishTime = o.UserFinishTime,
                                UserID = o.UserID,
                                WorkerFinishTime = o.WorkerFinishTime,
                                WorkerID = o.WorkerID,
                                BusinessName = b.Name,
                                PayerType = b.PayerType,
                                QuoteWays = b.QuoteWays
                            };

                return query.FirstOrDefault();
            }
        }

        /// <summary>
        /// 自动取消订单
        /// </summary>
        /// <param name="orderID"></param>
        public static bool AutoCancelOrder(long orderID)
        {
            using (var db = new DataContext())
            {
                var order = db.KylinService_Order.SingleOrDefault(p => p.OrderID == orderID);

                if (order.BusinessType == (int)BusinessServiceType.Visiting)
                {
                    order.Status = (int)VisitingServiceOrderStatus.Cancel;
                }
                else
                {
                    order.Status = (int)ReservationServiceOrderStatus.Cancel;
                }
                order.CancelTime = DateTime.Now;

                return db.SaveChanges() > 0;
            }
        }
    }
}

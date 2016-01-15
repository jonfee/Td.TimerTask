using KylinService.Data.Model;
using KylinService.Services.Appoint;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KylinService.Data.Provider
{
    public class AppointOrderProvider
    {
        /// <summary>
        /// 获取今天将要逾期确认服务结束的订单
        /// </summary>
        /// <param name="lateDays">延迟确认服务完成时间（单位：天数）</param>
        /// <returns></returns>
        public static List<AppointOrderModel> GetUserFinishLateListForTodayWillTimeout(int lateDays)
        {
            using (var db = new DataContext())
            {
                var query = from o in db.KylinService_Order
                            join b in db.KylinService_Business
                            on o.BusinessID equals b.BusinessID
                            where
                            (
                            (o.BusinessType == (int)AppointBusinessType.ShangMen && o.Status == (int)ShangMenOrderStatus.WorkerFinish)//上门服务且状态为服务人员已完成服务
                            || (o.BusinessType == (int)AppointBusinessType.YuYue && o.Status == (int)YuYueOrderStatus.WorkerFinish)//预约服务且状态为商家已完成服务
                            )
                            && o.WorkerFinishTime.HasValue && o.WorkerFinishTime.Value.AddDays(lateDays) <= DateTime.Now
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

                return query.ToList();
            }
        }

        /// <summary>
        /// 获取今天将要逾期支付的订单
        /// </summary>
        /// <param name="lateMinutes">延迟支付时间（单位：分钟）</param>
        /// <returns></returns>
        public static List<AppointOrderModel> GetPaymentLateListForTodayWillTimeout(int lateMinutes)
        {
            using (var db = new DataContext())
            {
                var query = from o in db.KylinService_Order
                            join b in db.KylinService_Business
                            on o.BusinessID equals b.BusinessID
                            where
                            (
                                (
                                    b.QuoteWays == (int)AppointQuoteWays.AtSubmit && o.CreateTime.AddMinutes(lateMinutes) <= DateTime.Now && //下单时计价 
                                    (
                                        (o.BusinessType == (int)AppointBusinessType.ShangMen && o.Status == (int)ShangMenOrderStatus.WaitReceiving)//上门订单等待商家接单
                                        || (o.BusinessType == (int)AppointBusinessType.YuYue && o.Status == (int)YuYueOrderStatus.WaitReceiving)//预约订单与状态匹配
                                    )
                                )//下单时计价，但已超时
                                ||
                                (
                                    b.QuoteWays == (int)AppointQuoteWays.AtDoorComeOn && o.ConfirmTime.HasValue && o.ConfirmTime.Value.AddMinutes(lateMinutes) <= DateTime.Now &&
                                    (
                                        (o.BusinessType == (int)AppointBusinessType.ShangMen && o.Status == (int)ShangMenOrderStatus.ConfirmStudio)//上门订单与状态匹配
                                        || (o.BusinessType == (int)AppointBusinessType.YuYue && o.Status == (int)YuYueOrderStatus.ConfirmStudio)//预约订单与状态匹配
                                    )
                                )//上门时计价，且用户已确定服务方案，但已超时
                            )
                            && o.PaiedTime.HasValue == false
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

                return query.ToList();
            }
        }

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

                if (order.BusinessType == (int)AppointBusinessType.ShangMen)
                {
                    order.Status = (int)ShangMenOrderStatus.Cancel;
                }
                else
                {
                    order.Status = (int)YuYueOrderStatus.Cancel;
                }
                order.CancelTime = DateTime.Now;

                return db.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 自动确定服务完成
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static bool AutoFinishByUser(long orderID)
        {
            return false;
        }
    }
}

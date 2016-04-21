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
                            (o.BusinessType == (int)BusinessServiceType.Visiting && o.Status == (int)VisitingServiceOrderStatus.WorkerServiceDone)//上门服务且状态为服务人员已完成服务
                            || (o.BusinessType == (int)BusinessServiceType.Reservation && o.Status == (int)ReservationServiceOrderStatus.MerchantServiceDone)//预约服务且状态为商家已完成服务
                            )
                            && o.WorkerFinishTime.HasValue && o.WorkerFinishTime.Value.AddDays(lateDays) < DateTime.Now.Date.AddDays(1)//&& o.WorkerFinishTime.Value.AddDays(lateDays).Date == DateTime.Now.Date
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
                                    b.QuoteWays == (int)BusinessServiceQuote.WhenOrder && o.CreateTime.AddMinutes(lateMinutes) < DateTime.Now.Date.AddDays(1) && //下单时计价  && o.CreateTime.AddMinutes(lateMinutes).Date == DateTime.Now.Date
                                    (
                                        (o.BusinessType == (int)BusinessServiceType.Visiting && o.Status == (int)VisitingServiceOrderStatus.WaitingMerchantReceive)//上门订单等待商家接单
                                        || (o.BusinessType == (int)BusinessServiceType.Reservation && o.Status == (int)ReservationServiceOrderStatus.WaitingMerchantReceive)//预约订单与状态匹配
                                    )
                                )//下单时计价，今天超时
                                ||
                                (
                                    b.QuoteWays == (int)BusinessServiceQuote.WhenMeeting && o.ConfirmTime.HasValue && o.ConfirmTime.Value.AddMinutes(lateMinutes) < DateTime.Now.Date.AddDays(1)//&& o.ConfirmTime.Value.AddMinutes(lateMinutes).Date == DateTime.Now.Date 
                                    &&
                                    (
                                        (o.BusinessType == (int)BusinessServiceType.Visiting && o.Status == (int)VisitingServiceOrderStatus.UserConfirmQuote)//上门订单与状态匹配
                                        || (o.BusinessType == (int)BusinessServiceType.Reservation && o.Status == (int)ReservationServiceOrderStatus.UserConfirmSolution)//预约订单与状态匹配
                                    )
                                )//上门时计价，且用户已确定服务方案，今天超时
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

        /// <summary>
        /// 自动确定服务完成
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static bool AutoFinishByUser(long orderID)
        {
            using (var db = new DataContext())
            {
                var order = db.KylinService_Order.SingleOrDefault(p => p.OrderID == orderID);

                #region 计算本次完成订单需要结算的金额

                decimal money = 0;

                //线下支付，但下单时有预约金
                if (order.PaymentType == (int)BusinessServiceOrderPayment.OffLine)
                {
                    if (order.PrepaidAmount > 0) money = order.PrepaidAmount;
                }
                else if (order.PaiedTime.HasValue)
                {
                    money = order.ActualOrderAmount;
                }
                else
                {
                    throw new Exception("当前订单数据异常（线上支付方式但未检测到付款信息），不能自动确定服务完成！");
                }

                #endregion

                #region 结算

                money = Math.Abs(money);

                //如果有款项需要结算
                if (money > 0)
                {
                    var business = db.KylinService_Business.SingleOrDefault(p => p.BusinessID == order.BusinessID);

                    //本订单是否应由下单方支付给服务方，否则为服务方支付给下单方
                    var payToServer = business.PayerType == (int)BusinessServicePayer.Servicer;

                    var user = db.User_Account.SingleOrDefault(p => p.UserID == order.UserID);

                    if (payToServer)
                    {
                        if (user.FreezeMoney < money) throw new Exception("程序猿大哥摊上大事了，用户冻结资金怎么不够本次订单结算时扣款呢？！");
                        user.FreezeMoney -= money;
                    }
                    else
                    {
                        user.Balance += money;

                        //交易记录
                        db.User_TradeRecords.Add(new User_TradeRecords
                        {
                            Amount = money,
                            CreateTime = DateTime.Now,
                            PaymentType = order.PaymentType,
                            TradeID = IDCreater.Instance.GetID(),
                            TradeInfo = string.Format("{0}服务中获利所得[订单编号：{1}]", business.Name, order.OrderCode),
                            TradeNo = string.Empty,
                            TradeType = (int)UserTransType.Proceeds,
                            UserID = user.UserID
                        });
                    }

                    if (order.ServerType == (int)BusinessOrderServiceProvider.Merchant)
                    {
                        var merchant = db.Merchant_Account.SingleOrDefault(p => p.MerchantID == order.MerchantID);
                        if (payToServer)//下单方支付给商家
                        {
                            merchant.Balance += money;
                            db.Merchant_TradeRecords.Add(new Merchant_TradeRecords
                            {
                                Amount = money,
                                CreateTime = DateTime.Now,
                                PaymentType = order.PaymentType,
                                TradeID = IDCreater.Instance.GetID(),
                                TradeInfo = string.Format("{0}服务中获利所得[订单编号：{1}]", business.Name, order.OrderCode),
                                TradeNo = string.Empty,
                                TradeType = (int)MerchantTransType.ServiceSales,
                                MerchantID = merchant.MerchantID
                            });
                        }
                        else//商家支付给下单方
                        {
                            merchant.FreezeMoney -= money;
                        }
                    }
                    else if (order.ServerType == (int)BusinessOrderServiceProvider.Person)
                    {
                        var worker = db.Worker_Account.SingleOrDefault(p => p.WorkerID == order.WorkerID);
                        if (payToServer)//下单方支付给服务人员
                        {
                            worker.Balance += money;
                            db.Worker_TradeRecords.Add(new Worker_TradeRecords
                            {
                                Amount = money,
                                CreateTime = DateTime.Now,
                                PaymentType = order.PaymentType,
                                TradeID = IDCreater.Instance.GetID(),
                                TradeInfo = string.Format("{0}服务中获利所得[订单编号：{1}]", business.Name, order.OrderCode),
                                TradeNo = string.Empty,
                                TradeType = (int)WorkerTransType.ServiceSales,
                                UserID = worker.WorkerID
                            });
                        }
                        else//服务人员支付给下单方
                        {
                            worker.FreezeMoney -= money;
                        }
                    }
                    else
                    {
                        throw new Exception("当前订单数据异常（不明确的接单方类型），不能自动确定服务完成！");
                    }
                }

                #endregion

                #region 更新訂單信息

                if (order.BusinessType == (int)BusinessServiceType.Visiting)
                {
                    order.Status = (int)VisitingServiceOrderStatus.UserServiceConfirmDone;
                }
                else if (order.BusinessType == (int)BusinessServiceType.Reservation)
                {
                    order.Status = (int)ReservationServiceOrderStatus.UserServiceConfirmDone;
                }
                else
                {
                    throw new Exception("当前订单数据异常（不明确的订单类型），不能自动确定服务完成！");
                }
                order.UserFinishTime = DateTime.Now;

                #endregion

                return db.SaveChanges() > 0;
            }
        }
    }
}

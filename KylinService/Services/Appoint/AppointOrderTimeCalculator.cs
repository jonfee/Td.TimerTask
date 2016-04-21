using KylinService.Data.Model;
using KylinService.SysEnums;
using System;
using Td.Kylin.EnumLibrary;

namespace KylinService.Services.Appoint
{
    /// <summary>
    /// 上门预约订单时间计算器
    /// </summary>
    public class AppointOrderTimeCalculator
    {
        /// <summary>
        /// 获取将要超时的时间点
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DateTime GetTimeoutTime(AppointOrderModel order, AppointConfig config, AppointLateType lateType)
        {
            DateTime timeout = DateTime.Now.Date;

            if (null != order)
            {
                switch (lateType)
                {
                    case AppointLateType.LateNoPayment:
                        if (order.QuoteWays == (int)BusinessServiceQuote.WhenOrder)
                        {
                            timeout = order.CreateTime.AddMinutes(config.PaymentWaitMinutes);
                        }
                        else if (order.QuoteWays == (int)BusinessServiceQuote.WhenMeeting)
                        {
                            timeout = order.ConfirmTime.Value.AddMinutes(config.PaymentWaitMinutes);
                        }
                        break;
                    case AppointLateType.LateUserFinish:
                        if (order.WorkerFinishTime.HasValue)
                        {
                            timeout = order.WorkerFinishTime.Value.AddDays(config.EndServiceWaitUserDays);
                        }
                        break;
                }
            }

            return timeout;
        }
    }
}

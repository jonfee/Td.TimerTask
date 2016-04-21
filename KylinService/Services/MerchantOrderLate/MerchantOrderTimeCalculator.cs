using KylinService.Data.Model;
using KylinService.Services.MerchantOrderLate;
using System;

namespace KylinService.Services.MerchantOrderLate
{
    /// <summary>
    /// 订单时间计算器
    /// </summary>
    public class MerchantOrderTimeCalculator
    {
        /// <summary>
        /// 获取将要超时的时间点
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DateTime GetTimeoutTime(MerchantOrderModel order, MerchantOrderLateConfig config, SysEnums.MerchantOrderLateType lateType)
        {
            DateTime timeout = DateTime.Now.Date;

            if (null != order)
            {
                switch (lateType)
                {
                    case SysEnums.MerchantOrderLateType.LateNoPayment:
                        timeout = order.CreateTime.AddMinutes(config.WaitPaymentMinutes);
                        break;
                    case SysEnums.MerchantOrderLateType.LateUserFinish:
                        if (order.SendTime.HasValue)
                        {
                            timeout = order.SendTime.Value.AddDays(config.WaitReceiptGoodsDays);
                        }
                        break;
                }
            }

            return timeout;
        }
    }
}

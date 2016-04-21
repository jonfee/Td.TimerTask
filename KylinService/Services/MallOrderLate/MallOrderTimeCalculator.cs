﻿using KylinService.Data.Model;
using System;
using Td.Kylin.EnumLibrary;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 订单时间计算器
    /// </summary>
    public class MallOrderTimeCalculator
    {
        /// <summary>
        /// 获取将要超时的时间点
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DateTime GetTimeoutTime(MallOrderModel order, B2COrderLateConfig config, SysEnums.MallOrderLateType lateType)
        {
            DateTime timeout = DateTime.Now.Date;

            if (null != order)
            {
                switch (lateType)
                {
                    case SysEnums.MallOrderLateType.LateNoPayment:
                        if (order.OrderType == (int)B2COrderType.ShakeOrder && order.NeedPayTime.HasValue)
                        {
                            timeout = order.NeedPayTime.Value;
                        }
                        else
                        {
                            timeout = order.CreateTime.AddMinutes(config.WaitPaymentMinutes);
                        }
                        break;
                    case SysEnums.MallOrderLateType.LateUserFinish:
                        if (order.ShipTime.HasValue)
                        {
                            timeout = order.ShipTime.Value.AddDays(config.WaitReceiptGoodsDays);
                        }
                        break;
                }
            }

            return timeout;
        }
    }
}

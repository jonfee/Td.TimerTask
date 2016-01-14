using KylinService.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static DateTime GetTimeoutTime(MallOrderModel order, OrderLateConfig config)
        {
            DateTime timeout = DateTime.Now.Date;

            if (null != order)
            {
                switch ((SysEnums.MallOrderStatus)order.OrderStatus)
                {
                    case SysEnums.MallOrderStatus.NoPay:
                        if (order.OrderType == (int)SysEnums.MallOrderType.ShakeBuy && order.NeedPayTime.HasValue)
                        {
                            timeout = order.NeedPayTime.Value.AddHours(config.WaitPaymentHours);
                        }
                        else
                        {
                            timeout = order.CreateTime.AddHours(config.WaitPaymentHours);
                        }
                        break;
                    case SysEnums.MallOrderStatus.WaitReceiptGoods:
                        timeout = order.ShipTime.Value.AddDays(config.WaitReceiptGoodsDays);
                        break;
                }
            }

            return timeout;
        }
    }
}

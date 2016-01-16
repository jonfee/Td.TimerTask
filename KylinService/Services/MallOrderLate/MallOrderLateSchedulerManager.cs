using KylinService.Core;
using KylinService.Data.Model;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 订单处理任务计划管理
    /// </summary>
    public class MallOrderLateSchedulerManager
    {
        //任务计划集合
        public static Hashtable Schedulers = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 开始一个订单处理任务计划
        /// </summary>
        /// <param name="config"></param>
        /// <param name="order"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public static void StartScheduler(SysEnums.MallOrderLateType lateType, OrderLateConfig config, MallOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(order.OrderID))
            {
                var schedule = Schedulers[order.OrderID] as BaseMallOrderLateScheduler;

                var oldTimeout = MallOrderTimeCalculator.GetTimeoutTime(schedule.Order, config, lateType);
                var newTimeout = MallOrderTimeCalculator.GetTimeoutTime(order, config, lateType);

                switch (lateType)
                {
                    case SysEnums.MallOrderLateType.LateNoPayment:
                        schedule = new MallOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        break;
                    case SysEnums.MallOrderLateType.LateUserFinish:
                        schedule = new MallOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        break;
                }

                if (oldTimeout != newTimeout)
                {
                    Schedulers[order.OrderID] = schedule;
                }
            }
            else
            {
                switch (lateType)
                {
                    case SysEnums.MallOrderLateType.LateNoPayment:
                        Schedulers.Add(order.OrderID, new MallOrderPaymentLateScheduler(config, order, form, writeDelegate));
                        break;
                    case SysEnums.MallOrderLateType.LateUserFinish:
                        Schedulers.Add(order.OrderID, new MallOrderPaymentLateScheduler(config, order, form, writeDelegate));
                        break;
                }
            }
        }

        /// <summary>
        /// 校正任务计划列表，将无效的计划移除
        /// </summary>
        /// <param name="orderIds"></param>
        public static void CheckScheduler(long[] orderIds)
        {
            if (null == orderIds || Schedulers.Keys.Count < 1) return;

            var scheduleIDs = Schedulers.Keys;

            foreach (var key in scheduleIDs)
            {
                var id = (long)key;

                if (!orderIds.Contains(id))
                {
                    Schedulers.Remove(key);
                }
            }
        }
    }
}

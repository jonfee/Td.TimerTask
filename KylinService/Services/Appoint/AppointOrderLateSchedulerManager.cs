using KylinService.Core;
using KylinService.Data.Model;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace KylinService.Services.Appoint
{
    /// <summary>
    /// 上门预约订单逾期处理任务计划管理
    /// </summary>
    public class AppointOrderLateSchedulerManager
    {
        //任务计划集合
        public static Hashtable Schedulers = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 开始一个订单处理任务计划
        /// </summary>
        /// <param name="lateType">逾期处理的类型</param>
        /// <param name="config"></param>
        /// <param name="order"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public static void StartScheduler(SysEnums.AppointLateType lateType, AppointConfig config, AppointOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(order.OrderID))
            {
                var oldSchedule = Schedulers[order.OrderID] as BaseAppointOrderLateScheduler;

                var oldTimeout = DateTime.Now.Date;
                var newTimeout = DateTime.Now.Date;

                switch (lateType)
                {
                    case SysEnums.AppointLateType.LateNoPayment:
                        oldTimeout = AppointOrderTimeCalculator.GetTimeoutTime(oldSchedule.Order, config, SysEnums.AppointLateType.LateNoPayment);
                        newTimeout = AppointOrderTimeCalculator.GetTimeoutTime(order, config,SysEnums.AppointLateType.LateNoPayment);
                        oldSchedule = new AppointOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        break;
                    case SysEnums.AppointLateType.LateUserFinish:
                        oldTimeout = AppointOrderTimeCalculator.GetTimeoutTime(oldSchedule.Order, config, SysEnums.AppointLateType.LateUserFinish);
                        newTimeout = AppointOrderTimeCalculator.GetTimeoutTime(order, config,SysEnums.AppointLateType.LateUserFinish);
                        oldSchedule = new AppointOrderUserFinishLateScheduler(config, order, form, writeDelegate);
                        break;
                }

                if (oldTimeout != newTimeout)
                {
                    Schedulers[order.OrderID] = oldSchedule;
                }
            }
            else
            {
                switch (lateType)
                {
                    case SysEnums.AppointLateType.LateNoPayment:
                       var nopayScheduler = new AppointOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        Schedulers.Add(order.OrderID, nopayScheduler);
                        break;
                    case SysEnums.AppointLateType.LateUserFinish:
                        var userfinishScheduler = new AppointOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        Schedulers.Add(order.OrderID, userfinishScheduler);
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

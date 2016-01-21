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
    public class AppointOrderLateSchedulerManager : BaseSchedulerManager
    {
        private static AppointOrderLateSchedulerManager _instance;

        private readonly static object myLock = new object();

        public static AppointOrderLateSchedulerManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (myLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppointOrderLateSchedulerManager();
                        }
                    }
                }

                return _instance;
            }
        }

        private AppointOrderLateSchedulerManager() { }

        /// <summary>
        /// 开始一个订单处理任务计划
        /// </summary>
        /// <param name="lateType">逾期处理的类型</param>
        /// <param name="config"></param>
        /// <param name="order"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public void StartScheduler(SysEnums.AppointLateType lateType, AppointConfig config, AppointOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(order.OrderID))
            {
                var schedule = Schedulers[order.OrderID] as BaseAppointOrderLateScheduler;

                var oldTimeout = AppointOrderTimeCalculator.GetTimeoutTime(schedule.Order, config, lateType);
                var newTimeout = AppointOrderTimeCalculator.GetTimeoutTime(order, config, lateType);

                switch (lateType)
                {
                    case SysEnums.AppointLateType.LateNoPayment:
                        schedule = new AppointOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        break;
                    case SysEnums.AppointLateType.LateUserFinish:
                        schedule = new AppointOrderUserFinishLateScheduler(config, order, form, writeDelegate);
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
                    case SysEnums.AppointLateType.LateNoPayment:
                        Schedulers.Add(order.OrderID, new AppointOrderPaymentLateScheduler(config, order, form, writeDelegate));
                        break;
                    case SysEnums.AppointLateType.LateUserFinish:
                        Schedulers.Add(order.OrderID, new AppointOrderUserFinishLateScheduler(config, order, form, writeDelegate));
                        break;
                }
            }
        }
    }
}

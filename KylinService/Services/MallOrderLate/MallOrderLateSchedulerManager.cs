using KylinService.Core;
using KylinService.Data.Model;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 订单处理任务计划管理
    /// </summary>
    public class MallOrderLateSchedulerManager:BaseSchedulerManager
    {
        private static MallOrderLateSchedulerManager _instance;

        private readonly static object myLock = new object();

        public static MallOrderLateSchedulerManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (myLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MallOrderLateSchedulerManager();
                        }
                    }
                }

                return _instance;
            }
        }

        private MallOrderLateSchedulerManager() { }

        /// <summary>
        /// 开始一个订单处理任务计划
        /// </summary>
        /// <param name="config"></param>
        /// <param name="order"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public void StartScheduler(SysEnums.MallOrderLateType lateType, B2COrderLateConfig config, MallOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(order.OrderID))
            {
                var schedule = Schedulers[order.OrderID] as BaseMallOrderLateScheduler;
                schedule.LateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                schedule.LateTimer.Dispose();

                var oldTimeout = MallOrderTimeCalculator.GetTimeoutTime(schedule.Order, config, lateType);
                var newTimeout = MallOrderTimeCalculator.GetTimeoutTime(order, config, lateType);

                switch (lateType)
                {
                    case SysEnums.MallOrderLateType.LateNoPayment:
                        schedule = new MallOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        break;
                    case SysEnums.MallOrderLateType.LateUserFinish:
                        schedule = new MallOrderLateUserFinishScheduler(config, order, form, writeDelegate);
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
                        Schedulers.Add(order.OrderID, new MallOrderLateUserFinishScheduler(config, order, form, writeDelegate));
                        break;
                }
            }
        }
    }
}

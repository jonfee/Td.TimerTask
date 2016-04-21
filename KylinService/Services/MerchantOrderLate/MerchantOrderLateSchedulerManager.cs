using KylinService.Core;
using KylinService.Data.Model;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.MerchantOrderLate
{
    /// <summary>
    /// 订单处理任务计划管理
    /// </summary>
    public class MerchantOrderLateSchedulerManager : BaseSchedulerManager
    {
        private static MerchantOrderLateSchedulerManager _instance;

        private readonly static object myLock = new object();

        public static MerchantOrderLateSchedulerManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (myLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MerchantOrderLateSchedulerManager();
                        }
                    }
                }

                return _instance;
            }
        }

        private MerchantOrderLateSchedulerManager() { }

        /// <summary>
        /// 开始一个订单处理任务计划
        /// </summary>
        /// <param name="config"></param>
        /// <param name="order"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public void StartScheduler(SysEnums.MerchantOrderLateType lateType, MerchantOrderLateConfig config, MerchantOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(order.OrderID))
            {
                var schedule = Schedulers[order.OrderID] as BaseMerchantOrderLateScheduler;
                schedule.LateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                schedule.LateTimer.Dispose();

                var oldTimeout = MerchantOrderTimeCalculator.GetTimeoutTime(schedule.Order, config, lateType);
                var newTimeout = MerchantOrderTimeCalculator.GetTimeoutTime(order, config, lateType);

                switch (lateType)
                {
                    case SysEnums.MerchantOrderLateType.LateNoPayment:
                        schedule = new MerchantOrderPaymentLateScheduler(config, order, form, writeDelegate);
                        break;
                    case SysEnums.MerchantOrderLateType.LateUserFinish:
                        schedule = new MerchantOrderLateUserFinishScheduler(config, order, form, writeDelegate);
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
                    case SysEnums.MerchantOrderLateType.LateNoPayment:
                        Schedulers.Add(order.OrderID, new MerchantOrderPaymentLateScheduler(config, order, form, writeDelegate));
                        break;
                    case SysEnums.MerchantOrderLateType.LateUserFinish:
                        Schedulers.Add(order.OrderID, new MerchantOrderLateUserFinishScheduler(config, order, form, writeDelegate));
                        break;
                }
            }
        }
    }
}

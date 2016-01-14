using KylinService.Core;
using KylinService.Data.Model;
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
        public static void StartScheduler(OrderLateConfig config, MallOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(order.OrderID))
            {
                var oldSchedule = Schedulers[order.OrderID] as MallOrderLateScheduler;

                var oldTimeout = MallOrderTimeCalculator.GetTimeoutTime(oldSchedule.Order, config);

                var newTimeout = MallOrderTimeCalculator.GetTimeoutTime(order, config);

                if (oldTimeout != newTimeout)
                {
                    oldSchedule = new MallOrderLateScheduler(config,order, form, writeDelegate);
                    Schedulers[order.OrderID] = oldSchedule;
                }
            }
            else
            {
                var schedule = new MallOrderLateScheduler(config, order, form, writeDelegate);

                Schedulers.Add(order.OrderID, schedule);
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

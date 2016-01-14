using KylinService.Core;
using KylinService.Data.Model;
using System.Collections;
using System.Windows.Forms;
using System.Linq;

namespace KylinService.Services.WelfareLottery
{
    public class WelfareLotterySchedulerManager
    {
        /// <summary>
        /// 开奖的任务计划列表
        /// </summary>
        public static Hashtable Schedulers = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 开始一个福利开奖任务计划
        /// </summary>
        /// <param name="welfare"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public static void StartScheduler(WelfareModel welfare, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(welfare.PhaseID))
            {
                var oldSchedule = Schedulers[welfare.PhaseID] as LotteryScheduler;

                if (welfare.LotteryTime != oldSchedule.Welfare.LotteryTime)
                {
                    oldSchedule = new LotteryScheduler(welfare, form, writeDelegate);
                    Schedulers[welfare.PhaseID] = oldSchedule;
                }
            }
            else
            {
                var schedule = new LotteryScheduler(welfare, form, writeDelegate);

                Schedulers.Add(welfare.PhaseID, schedule);
            }
        }

        /// <summary>
        /// 校正任务计划列表，将无效的计划移除
        /// </summary>
        /// <param name="phaseIds"></param>
        public static void CheckScheduler(long[] phaseIds)
        {
            if (null == phaseIds || Schedulers.Keys.Count < 1) return;

            var scheduleIDs = Schedulers.Keys;

            foreach (var key in scheduleIDs)
            {
                var id = (long)key;

                if (!phaseIds.Contains(id))
                {
                    Schedulers.Remove(key);
                }
            }
        }
    }
}

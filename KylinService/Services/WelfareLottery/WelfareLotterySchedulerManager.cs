using KylinService.Core;
using KylinService.Data.Model;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.WelfareLottery
{
    /// <summary>
    /// 限时福利开奖任务计划管理器
    /// </summary>
    public class WelfareLotterySchedulerManager : BaseSchedulerManager
    {
        private static WelfareLotterySchedulerManager _instance;

        private readonly static object myLock = new object();

        public static WelfareLotterySchedulerManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (myLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new WelfareLotterySchedulerManager();
                        }
                    }
                }

                return _instance;
            }
        }

        private WelfareLotterySchedulerManager() { }

        /// <summary>
        /// 开始一个福利开奖任务计划
        /// </summary>
        /// <param name="welfare"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public void StartScheduler(WelfareModel welfare, Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            if (Schedulers.ContainsKey(welfare.PhaseID))
            {
                var oldSchedule = Schedulers[welfare.PhaseID] as LotteryScheduler;

                if (welfare.LotteryTime != oldSchedule.Welfare.LotteryTime)
                {
                    oldSchedule.LotteryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    oldSchedule.LotteryTimer.Dispose();
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
    }
}

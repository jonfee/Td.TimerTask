using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.SysEnums;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace KylinService.Services.WelfareLottery
{
    /// <summary>
    /// 福利开奖服务
    /// </summary>
    public class WelfareLotteryService : BaseService
    {
        public WelfareLotteryService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.ServiceType = ScheduleType.WelfareLottery.ToString();
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="parameters"></param>
        protected override void OnStart(params object[] parameters)
        {
            base.OnStart();

            string beforeMessage = string.Format("{0} 限时福利数据统计中……", ServiceName);
            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, beforeMessage);

            //获取当日待开奖的福利活动
            var list = WelfareProvider.GetTodayWaittingLotteryList();

            //写入开奖计划任务
            if (null != list && list.Count > 0)
            {
                WelfareLotterySchedulerManager.CheckScheduler(list.Select(p => p.PhaseID).ToArray());

                foreach (var welfare in list)
                {
                    if (null != welfare)
                    {
                        WelfareLotterySchedulerManager.StartScheduler(welfare, this.CurrentForm, this.WriteDelegate);
                    }
                }
            }

            //获取福利开奖的任务计划集合
            ICollection scheduleList = WelfareLotterySchedulerManager.Schedulers.Values;

            //输出待开奖数量及福利情况
            string message = string.Format("限时福利数据统计完成，今日共有 {0} 个福利等待开奖！分别是：", scheduleList.Count);
            DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);

            foreach (var val in scheduleList)
            {
                var schedule = val as LotteryScheduler;

                var welfare = schedule.Welfare;

                var dueTime = welfare.LotteryTime - DateTime.Now;

                string welPut = string.Format("【福利活动：{0}】将在{1}小时{2}分{3}秒后开奖", welfare.WelfareName, dueTime.Hours, dueTime.Minutes, dueTime.Seconds);

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, welPut);
            }
        }
    }
}

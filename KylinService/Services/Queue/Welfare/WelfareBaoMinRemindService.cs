using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Push.Model;
using KylinService.Redis.Schedule;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Welfare
{
    public sealed class WelfareBaoMinRemindService : QueueSchedulerService
    {
        /// <summary>
        /// 任务计划收集器
        /// </summary>

        SchedulerCollection collection;
        /// <summary>
        /// 任务计划数据所在Redis配置
        /// </summary>
        ScheduleRedisConfig config;

        public WelfareBaoMinRemindService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.WelfareBaoMinRemind, form, writeDelegate)
        {
            collection = new SchedulerCollection();
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.WelfareBaoMinRemind];
        }


        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<WelfareRemindModel>(config.Key) : null;

                    if (null != model)
                    {
                        int duetime = (int)model.ApplyStartTime.AddMinutes(-Startup.WelfareConfig.BeforeRemindMinutes).Subtract(DateTime.Now).TotalMilliseconds;    //延迟执行时间（以毫秒为单位）

                        if (duetime < 0) duetime = 0;

                        System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, Timeout.Infinite);

                        Schedulers.Add(model.WelfareID, timer);
                    }

                    //休眠100毫秒，避免CPU空转
                    Thread.Sleep(100);
                }
            });
        }

        protected override void Execute(object state)
        {
            var model = state as WelfareRemindModel;

            if (null == model) return;

            try
            {
                var lastWelfare = WelfareProvider.GetWelfare(model.WelfareID);

                if (null == lastWelfare || lastWelfare.IsDelete == true) throw new Exception("福利不存在或已被删除");

                if (lastWelfare.Status != (int)WelfareStatus.InProgress) throw new Exception("福利状态异常，不能被提醒");

                if (model.ApplyStartTime != lastWelfare.ApplyStartTime) throw new Exception("福利开始报名时间异常");

                #region 推送消息给需要提醒的用户

                //获取需要提醒的用户
                IEnumerable<WelfareRemindContent> remindContentList = WelfareProvider.GetRemindContentList(model.WelfareID);

                if (null != remindContentList && remindContentList.Count() > 0)
                {
                    var pushRedis = Startup.PushRedisConfigs[RedisPushType.WelfareRemind];

                    if (null != pushRedis)
                    {
                        pushRedis.DataBase.ListRightPush<WelfareRemindContent>(pushRedis.Key, remindContentList);
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }
            finally
            {
                Schedulers.Remove(model.WelfareID);
            }
        }
    }
}

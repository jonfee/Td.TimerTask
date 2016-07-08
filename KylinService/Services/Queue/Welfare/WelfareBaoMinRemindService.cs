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

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            //获取一条待处理数据
            var model = null != config ? config.DataBase.ListLeftPop<WelfareRemindModel>(config.Key) : null;

            if (null != model)
            {
                //非脏数据，则处理
                if (model.WelfareID.ToString() != Startup.DirtyDataPKValue)
                {
                    TimeSpan duetime = model.ApplyStartTime.AddMinutes(-Startup.WelfareConfig.BeforeRemindMinutes).Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                    if (duetime.Ticks < 0) duetime = TimeoutZero;

                    System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                    //输出消息
                    string message = string.Format("福利(ID:{0})将在{1}天{2}小时{3}分{4}秒后提醒用户参与报名", model.WelfareID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);
                    OutputMessage(message);

                    Schedulers.Add(model.WelfareID, timer);
                }

                return true;
            }

            return false;
        }

        protected override void Execute(object state)
        {
            var model = state as WelfareRemindModel;

            if (null == model) return;

            try
            {
                var lastWelfare = WelfareProvider.GetWelfare(model.WelfareID);

                if (null == lastWelfare || lastWelfare.IsDelete == true) throw new CustomException(string.Format("〖福利ID：{0}〗不存在或已被删除", model.WelfareID));

                if (lastWelfare.Status != (int)WelfareStatus.InProgress) throw new CustomException(string.Format("〖福利：{0}〗状态异常，不能被提醒", lastWelfare.WelfareName));

                if (model.ApplyStartTime != lastWelfare.ApplyStartTime) throw new CustomException(string.Format("〖福利：{0}〗开始报名时间异常", lastWelfare.WelfareName));

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

        protected override void WriteDirtyData()
        {
            var model = new WelfareRemindModel();
            model.WelfareID = long.Parse(Startup.DirtyDataPKValue);

            config.DataBase.ListRightPush(config.Key, model);
        }
    }
}

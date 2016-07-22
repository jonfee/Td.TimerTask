using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Push;
using KylinService.Redis.Push.Model;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Welfare
{
    public sealed class WelfareBaoMinRemindService : QueueSchedulerService<WelfareRemindModel>
    {

        public WelfareBaoMinRemindService() : base(QueueScheduleType.WelfareBaoMinRemind) { }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            if (null == RedisConfig) return false;

            if (null == QuequDatabase)
            {
                WriteMessageHelper.WriteMessage("Redis(database)连接丢失，source:" + this.ServiceName + "，Method:" + this.Me());
                return false;
            }

            //获取一条待处理数据
            var model = QuequDatabase.ListLeftPop<WelfareRemindModel>(RedisConfig.Key);

            return EntityTaskHandler(model);
        }

        protected override void Execute(object state)
        {
            var model = state as WelfareRemindModel;

            if (null == model) return;

            try
            {
                //从备份区将备份删除
                DeleteBackAfterDone(model.WelfareID);

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
                        var pushDb = PushRedisContext.Redis.GetDatabase(pushRedis.DbIndex);

                        if (pushDb != null)
                        {
                            pushDb.ListRightPush<WelfareRemindContent>(pushRedis.Key, remindContentList);
                        }
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

        protected override bool EntityTaskHandler(WelfareRemindModel model, bool mustBackup = true)
        {
            if (null != model)
            {
                TimeSpan duetime = model.ApplyStartTime.AddMinutes(-Startup.WelfareConfig.BeforeRemindMinutes).Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                if (duetime.Ticks < 0) duetime = TimeoutZero;

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                if (mustBackup)
                {
                    //复制到备份区以防数据丢失
                    BackBeforeDone(model.WelfareID, model);
                }

                //输出消息
                string message = string.Format("〖福利：{0}〗将在{1}天{2}小时{3}分{4}秒后提醒用户参与报名", model.WelfareID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);

                RunLogger(message);

                Schedulers.Add(model.WelfareID, timer);

                return true;
            }

            return false;
        }
    }
}

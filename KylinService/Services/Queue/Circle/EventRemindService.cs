using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Push.Model;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Circle
{
    /// <summary>
    /// 活动提醒服务
    /// </summary>
    public sealed class EventRemindService : QueueSchedulerService<CircleEventRemindModel>
    {

        public EventRemindService() : base(QueueScheduleType.CircleEventRemind) { }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            if (null == RedisConfig) return false;

            if (null == RedisConfig.DataBase)
            {
                WriteMessageHelper.WriteMessage("Redis(database)连接丢失，source:" + this.ServiceName + "，Method:" + this.Me());
                return false;
            }

            //获取一条待处理数据
            var model = RedisConfig.DataBase.ListLeftPop<CircleEventRemindModel>(RedisConfig.Key);

            return EntityTaskHandler(model);
        }

        protected override void Execute(object state)
        {
            var model = state as CircleEventRemindModel;

            if (null == model) return;

            try
            {
                //从备份区将备份删除
                DeleteBackAfterDone(model.EventID);

                var lastEvent = CircleProvider.GetEvent(model.EventID);

                if (null == lastEvent) throw new CustomException(string.Format("〖社区活动（ID:{0}）〗不存在或已被删除", model.EventID));

                if (lastEvent.EventStatus == (int)CircleEventStatus.Canceled) throw new CustomException(string.Format("〖社区活动（ID:{0}）〗已被取消", lastEvent.EventID));

                if (model.StartTime != lastEvent.StartTime) throw new CustomException(string.Format("〖社区活动（ID:{0}）〗开始时间异常", lastEvent.EventID));

                #region 推送消息给需要提醒的用户

                //获取需要提醒的用户
                IEnumerable<CircleEventRemindContent> remindContentList = CircleProvider.GetRemindContentList(model.EventID);

                if (null != remindContentList && remindContentList.Count() > 0)
                {
                    var pushRedis = Startup.PushRedisConfigs[RedisPushType.CircleEventRemind];

                    if (null != pushRedis)
                    {
                        pushRedis.DataBase.ListRightPush<CircleEventRemindContent>(pushRedis.Key, remindContentList);
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
                Schedulers.Remove(model.EventID);
            }
        }

        protected override bool EntityTaskHandler(CircleEventRemindModel model, bool mustBackup = true)
        {
            if (null != model)
            {
                TimeSpan duetime = model.StartTime.AddMinutes(-Startup.CircleConfig.BeforeRemindMinutes).Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                if (duetime.Ticks < 0) duetime = TimeoutZero;

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                if (mustBackup)
                {
                    //复制到备份区以防数据丢失
                    BackBeforeDone(model.EventID, model);
                }

                //输出消息
                string message = string.Format("〖社区活动（ID:{0}）〗将在{1}天{2}小时{3}分{4}秒后提醒用户", model.EventID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);

                Logger(message);

                Schedulers.Add(model.EventID, timer);

                return true;
            }

            return false;
        }
    }
}

using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Push;
using KylinService.Redis.Push.Model;
using KylinService.Redis.Schedule;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Welfare
{
    /// <summary>
    /// 福利开奖服务
    /// </summary>
    public sealed class LotteryService : QueueSchedulerService<WelfareLotteryModel>
    {
        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public LotteryService() : base(QueueScheduleType.WelfareLottery) { }

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
            var model = QuequDatabase.ListLeftPop<WelfareLotteryModel>(RedisConfig.Key);

            return EntityTaskHandler(model);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="state"></param>
        protected override void Execute(object state)
        {
            var model = state as WelfareLotteryModel;

            if (null == model) return;

            try
            {
                //从备份区将备份删除
                DeleteBackAfterDone(model.WelfareID);

                var lastWelfare = WelfareProvider.GetWelfare(model.WelfareID);

                #region //验证开奖的有效性

                if (null == lastWelfare) throw new CustomException(string.Format("〖福利：{0}〗已不存在！", model.Name));

                if (DateTime.Now >= lastWelfare.ExpiryEndTime) throw new CustomException(string.Format("〖福利：{0}〗已失效，不能被开奖！", lastWelfare.WelfareName));

                if (lastWelfare.Status != (int)WelfareStatus.InProgress) throw new CustomException(string.Format("无效的开奖请求，〖福利：{0}〗不被允许开奖！", lastWelfare.WelfareName));

                if (lastWelfare.IsDelete == true) throw new CustomException(string.Format("〖福利：{0}〗已被下架，不能开奖！", lastWelfare.WelfareName));

                if (model.LotteryTime != lastWelfare.LotteryTime) throw new CustomException(string.Format("〖福利：{0}〗不明确的开奖时间（在{1}与{2}之间不明确）！", lastWelfare.WelfareName, model.LotteryTime, lastWelfare.LotteryTime));

                bool timeRight = DateTime.Now.AddMilliseconds(-Startup.ErrorRangeMillisecond) <= lastWelfare.LotteryTime && lastWelfare.LotteryTime <= DateTime.Now.AddMilliseconds(Startup.ErrorRangeMillisecond);

                if (!timeRight) throw new CustomException(string.Format("〖福利：{0}〗开奖时间（{1}）无效！", lastWelfare.WelfareName, lastWelfare.LotteryTime.ToString("yyyy/MM/dd HH:mm:ss")));

                if (lastWelfare.WinNumber > 0) throw new CustomException(string.Format("〖福利：{0}〗活动不能重复开奖！", lastWelfare.WelfareName));

                //if (lastWelfare.PartNumber < 1) throw new Exception("没有报名参与的人员，不能开奖！");

                #endregion

                #region //获取活动的所有参与编号
                string[] partCodes = WelfareProvider.GetAllPartCode(model.WelfareID);
                #endregion

                #region //开奖并得到中奖编号集合

                //中奖的编号
                string[] winnerPartCodes = null;

                if (partCodes.Length <= lastWelfare.Number)
                {
                    winnerPartCodes = partCodes;
                }
                else
                {
                    LotteryContext context = new LotteryContext(partCodes, lastWelfare.Number);
                    context.Run();

                    winnerPartCodes = context.LotteryResult;
                }

                #endregion

                #region //写入中奖结果

                WelfareWinnerContent pushContent = WelfareProvider.WriteLotteryResult(model.WelfareID, winnerPartCodes);

                if (null != pushContent)
                {
                    string sucMessage = string.Format("〖福利：{0}〗已开奖，本次共有 {1} 名人员中奖（总参与人数：{2}）！", model.Name, winnerPartCodes.Length, lastWelfare.PartNumber);

                    RunLogger(sucMessage);
                }
                else
                {
                    throw new CustomException(string.Format("〖福利：{0}〗开奖失败！", lastWelfare.WelfareName));
                }

                #endregion

                #region //推送消息给中奖用户

                var pushRedis = Startup.PushRedisConfigs[RedisPushType.WelfareLottery];

                if (null != pushRedis)
                {
                    var pushDb = PushRedisContext.Redis.GetDatabase(pushRedis.DbIndex);

                    if (pushDb != null)
                    {
                        pushDb.ListRightPush(pushRedis.Key, pushContent);
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

        protected override bool EntityTaskHandler(WelfareLotteryModel model, bool mustBackup = true)
        {
            if (null != model)
            {
                TimeSpan duetime = model.LotteryTime.Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                if (duetime.Ticks < 0) duetime = TimeoutZero;

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                if (mustBackup)
                {
                    //复制到备份区以防数据丢失
                    BackBeforeDone(model.WelfareID, model);
                }

                //输出消息
                string message = string.Format("〖福利：{0}〗将于{2}天{3}小时{4}分{5}秒后（{1}）开奖", model.Name, model.LotteryTime.ToString("yyyy/MM/dd HH:mm:ss"), duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);

                RunLogger(message);

                Schedulers.Add(model.WelfareID, timer);

                return true;
            }

            return false;
        }
    }
}

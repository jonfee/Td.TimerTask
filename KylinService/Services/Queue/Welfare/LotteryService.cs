using KylinService.Core;
using KylinService.Data.Provider;
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
    public sealed class LotteryService : QueueSchedulerService
    {
        /// <summary>
        /// 任务计划数据所在Redis配置
        /// </summary>
        ScheduleRedisConfig config;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public LotteryService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.WelfareLottery, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.WelfareLottery];
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<WelfareLotteryModel>(config.Key) : null;

                    if (null != model)
                    {
                        int duetime = (int)model.LotteryTime.Subtract(DateTime.Now).TotalMilliseconds;    //延迟执行时间（以毫秒为单位）

                        if (duetime < 0) duetime = 0;

                        System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, Timeout.Infinite);

                        //输出消息
                        string message = string.Format("福利“{0}”将于{1}开奖", model.Name, model.LotteryTime.ToString("yyyy/MM/dd HH:mm:ss"));
                        OutputMessage(message);

                        Schedulers.Add(model.WelfareID, timer);
                    }

                    //休眠100毫秒，避免CPU空转
                    Thread.Sleep(100);
                }
            });
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
                var lastWelfare = WelfareProvider.GetWelfare(model.WelfareID);

                #region //验证开奖的有效性

                if (null == lastWelfare) throw new Exception(string.Format("〖福利：{0}〗已不存在！", model.Name));

                if (DateTime.Now >= lastWelfare.ExpiryEndTime) throw new Exception(string.Format("〖福利：{0}〗已失效，不能被开奖！", lastWelfare.WelfareName));

                if (lastWelfare.Status != (int)WelfareStatus.InProgress) throw new Exception(string.Format("无效的开奖请求，〖福利：{0}〗不被允许开奖！", lastWelfare.WelfareName));

                if (lastWelfare.IsDelete == true) throw new Exception(string.Format("〖福利：{0}〗已被下架，不能开奖！", lastWelfare.WelfareName));

                if (model.LotteryTime != lastWelfare.LotteryTime) throw new Exception(string.Format("〖福利：{0}〗不明确的开奖时间（在{1}与{2}之间不明确）！", lastWelfare.WelfareName, model.LotteryTime, lastWelfare.LotteryTime));

                if (DateTime.Now < lastWelfare.LotteryTime) throw new Exception(string.Format("〖福利：{0}〗开奖时间（{1}）未到！", lastWelfare.WelfareName, lastWelfare.LotteryTime.ToString("yyyy/MM/dd HH:mm:ss")));

                if (lastWelfare.WinNumber > 0) throw new Exception(string.Format("〖福利：{0}〗活动不能重复开奖！", lastWelfare.WelfareName));

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

                    OutputMessage(sucMessage);
                }
                else
                {
                    throw new Exception(string.Format("〖福利：{0}〗开奖失败！", lastWelfare.WelfareName));
                }

                #endregion

                #region //推送消息给中奖用户

                var pushRedis = Startup.PushRedisConfigs[RedisPushType.WelfareLottery];

                if (null != pushRedis)
                {
                    pushRedis.DataBase.ListRightPush(pushRedis.Key, pushContent);
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

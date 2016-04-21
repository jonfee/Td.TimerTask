﻿using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using KylinService.Redis;
using KylinService.Redis.Models;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.WelfareLottery
{
    /// <summary>
    /// 福利开奖计划
    /// </summary>
    public class LotteryScheduler : IScheduler
    {
        /// <summary>
        /// 福利数据
        /// </summary>
        public WelfareModel Welfare { get; private set; }

        /// <summary>
        /// 定时器
        /// </summary>
        public System.Threading.Timer LotteryTimer { get; private set; }

        public LotteryScheduler(WelfareModel welfare, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.Welfare = welfare;

            this.Key = null != this.Welfare ? this.Welfare.WelfareID.ToString() : string.Empty;

            if (!string.IsNullOrWhiteSpace(Key) && Welfare.LotteryTime > DateTime.Now)
            {
                //计算离开奖时间的时间差
                TimeSpan dueTime = Welfare.LotteryTime - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                LotteryTimer = new System.Threading.Timer((obj) =>
                  {
                      //计划执行（开奖）
                      this.Start();

                      LotteryTimer.Dispose();
                      LotteryTimer = null;

                  }, null, (int)Math.Ceiling(dueTime.TotalMilliseconds), Timeout.Infinite);
            }
        }

        /// <summary>
        /// 执行程序
        /// </summary>
        protected override void Execute()
        {
            if (null == Welfare) return;

            WelfareWinnerContent pushContent = null;

            try
            {
                var lastWelfare = WelfareProvider.GetWelfare(Welfare.WelfareID);

                #region //验证开奖的有效性

                if (null == lastWelfare) throw new Exception("福利信息已不存在！");

                if (DateTime.Now >= lastWelfare.ExpiryEndTime) throw new Exception("福利已失效，不能被开奖！");

                if (lastWelfare.Status != (int)WelfareStatus.InProgress) throw new Exception("无效的开奖请求，福利不被允许开奖！");

                if (lastWelfare.IsDelete == true) throw new Exception("福利已被下架，不能开奖！");

                if (DateTime.Now < lastWelfare.LotteryTime) throw new Exception("开奖时间未到，不能提前开奖！");

                if (lastWelfare.WinNumber > 0) throw new Exception("福利活动不能重复开奖！");

                //if (lastWelfare.PartNumber < 1) throw new Exception("没有报名参与的人员，不能开奖！");

                #endregion

                #region //获取活动的所有参与编号
                string[] partCodes = WelfareProvider.GetAllPartCode(Welfare.WelfareID);
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

                pushContent = WelfareProvider.WriteLotteryResult(Welfare.WelfareID, winnerPartCodes);

                if (null != pushContent)
                {
                    string sucMessage = string.Format("〖福利：{0}〗已开奖，本次共有 {1} 名参与人员中奖！", Welfare.WelfareName, winnerPartCodes.Count());

                    DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, sucMessage);
                }
                else
                {
                    throw new Exception("开奖失败！");
                }

                #endregion
            }
            catch (Exception ex)
            {
                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, ex.Message);
            }

            if (null != pushContent)
            {
                try
                {
                    var keyDb = RedisConfigManager.GetSaveKeyDbConfig(SysEnums.RedisSaveType.WelfareLottery);

                    if (null != keyDb)
                    {
                        var db = RedisManager.Redis.GetDatabase(keyDb.DBindex);

                        db.ListRightPush(keyDb.Key, pushContent);
                    }
                }
                catch (Exception ex)
                {
                    DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, ex.Message);
                }
            }
        }
    }
}

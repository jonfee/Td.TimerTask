using KylinService.Data.Entity;
using KylinService.Data.Model;
using KylinService.Redis.Models;
using KylinService.Services.WelfareLottery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KylinService.Data.Provider
{
    /// <summary>
    /// 限时福利分期活动
    /// </summary>
    public class WelfareProvider
    {
        /// <summary>
        /// 获取当天等待开奖的福利
        /// </summary>
        /// <returns></returns>
        public static List<WelfareModel> GetTodayWaittingLotteryList()
        {
            using (var db = new DataContext())
            {
                var lastTime = DateTime.Now.Date.AddDays(1);

                var query = from p in db.Welfare_Phases
                            join w in db.Merchant_Welfare
                            on p.WelfareID equals w.WelfareID
                            where p.Enabled == true && p.LotteryTime > DateTime.Now && p.LotteryTime < lastTime
                            select new WelfareModel
                            {
                                Enabled = p.Enabled,
                                LotteryTime = p.LotteryTime,
                                MarchantName = w.MerchantName,
                                Number = p.Number,
                                PartNumber = p.PartNumber,
                                PhaseID = p.PhasesID,
                                WelfareID = p.WelfareID,
                                WelfareName = w.Name,
                                WelfareType = p.WelfareType,
                                WinNumber = p.WinNumber,
                                IsDelete = w.IsDelete,
                                ExpiryStartTime = w.ExpiryStartTime,
                                ExpiryEndTime = w.ExpiryEndTime,
                                Status = w.Status
                            };

                return query.OrderBy(p => p.LotteryTime).ToList();
            }
        }

        /// <summary>
        /// 获取福利信息
        /// </summary>
        /// <param name="phaseID"></param>
        /// <returns></returns>
        public static WelfareModel GetWelfare(long phaseID)
        {
            using (var db = new DataContext())
            {
                var query = from p in db.Welfare_Phases
                            join w in db.Merchant_Welfare
                            on p.WelfareID equals w.WelfareID
                            where p.PhasesID == phaseID
                            select new WelfareModel
                            {
                                Enabled = p.Enabled,
                                LotteryTime = p.LotteryTime,
                                MarchantName = w.MerchantName,
                                Number = p.Number,
                                PartNumber = p.PartNumber,
                                PhaseID = p.PhasesID,
                                WelfareID = p.WelfareID,
                                WelfareName = w.Name,
                                WelfareType = p.WelfareType,
                                WinNumber = p.WinNumber,
                                IsDelete = w.IsDelete,
                                ExpiryStartTime = w.ExpiryStartTime,
                                ExpiryEndTime = w.ExpiryEndTime,
                                Status = w.Status
                            };

                return query.FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取福利活动的所有参与编号
        /// </summary>
        /// <param name="phaesID"></param>
        /// <returns></returns>
        public static string[] GetAllPartCode(long phaesID)
        {
            using (var db = new DataContext())
            {
                return db.Welfare_PartUser.Where(p => p.PhasesID == phaesID).Select(p => p.PartCode).ToArray();
            }
        }

        /// <summary>
        /// 写入开奖结果,并将福利放置用户名下等待用户领取（优惠券自动领取）
        /// </summary>
        /// <param name="phasesID"></param>
        /// <param name="partCodes"></param>
        public static WelfareWinnerContent WriteLotteryResult(long phasesID, string[] partCodes)
        {
            using (var db = new DataContext())
            {
                var phases = db.Welfare_Phases.SingleOrDefault(p => p.PhasesID == phasesID);

                var welfare = db.Merchant_Welfare.SingleOrDefault(p => p.WelfareID == phases.WelfareID);

                //是否应立即自动领取
                bool isNowAward = welfare.WelfareType == (int)SysEnums.WelfareType.Coupon;

                //中奖人员集合
                var winners = new List<Welfare_PartUser>();

                #region 存在参与人员，才会有中奖名单
                if (null != partCodes && partCodes.Length > 0)
                {
                    winners = db.Welfare_PartUser.Where(p => p.PhasesID == phasesID && partCodes.Contains(p.PartCode)).ToList();

                    winners.ForEach((item) =>
                    {
                        #region 更新中奖用户的参与信息

                        item.IsWin = true;
                        if (isNowAward)
                        {
                            item.IsAward = true;
                            item.AwardTime = DateTime.Now;
                        }
                        #endregion

                        #region 将中奖的福利放入用户名下
                        var userWelfare = new User_Welfare
                        {
                            AwardTime = isNowAward ? item.AwardTime : null,
                            ConsumerCode = ConsumerCodeGenerater.Instance.GetConsumerCode(),//消费码
                            CreateTime = DateTime.Now,
                            ExpiryEndTime = welfare.ExpiryEndTime,
                            ExpiryStartTime = welfare.ExpiryStartTime,
                            IsAward = isNowAward,
                            IsDelete = false,
                            IsUsed = false,
                            MerchantID = welfare.MerchantID,
                            MerchantName = welfare.MerchantName,
                            Name = welfare.Name,
                            PartCode = item.PartCode,
                            Picture = welfare.Picture,
                            Tag = welfare.Tag,
                            UserID = item.UserID,
                            UseTime = null,
                            WelfareID = welfare.WelfareID,
                            WelfareType = welfare.WelfareType
                        };
                        db.User_Welfare.Add(userWelfare);
                        #endregion
                    });

                    if (null != winners && winners.Count > 0) db.Welfare_PartUser.AttachRange(winners);
                }
                #endregion

                #region 更新福利信息

                //中奖人数
                phases.WinNumber = winners.Count;
                //领取人数
                if (isNowAward)
                {
                    phases.AcceptNumber = winners.Count;
                }

                //本次开奖剩余的福利数
                int surplusNumber = phases.Number - winners.Count;

                //剩余福利数
                welfare.SurplusNumber += surplusNumber;
                //总中奖人数
                welfare.WinNumber += winners.Count;
                //总领取人数
                if (isNowAward)
                {
                    welfare.DrawNumber += winners.Count;
                }
                if (welfare.SurplusNumber <= 0)
                {
                    //全部开奖完成后结束
                    welfare.Status = (int)SysEnums.WelfareStatus.Finish;
                }

                #endregion

                if (db.SaveChanges() > 0)
                {
                    return new WelfareWinnerContent
                    {
                        LolleryTime = phases.LotteryTime,
                        MerchantID = welfare.MerchantID,
                        MerchantName = welfare.MerchantName,
                        UserIDs = winners.Select(p => p.UserID).ToArray(),
                        WelfareID = phases.WelfareID,
                        WelfareName = welfare.Name,
                        WelfarePhaseID = phases.PhasesID,
                        WelfareType = phases.WelfareType
                    };
                }
                else
                {
                    return null;
                }
            }
        }
    }
}

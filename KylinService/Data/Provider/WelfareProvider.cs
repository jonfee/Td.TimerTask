using KylinService.Data.Model;
using KylinService.Redis.Models;
using KylinService.Services.WelfareLottery;
using System;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Provider
{
    /// <summary>
    /// 限时福利活动
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

                var query = from p in db.Merchant_Welfare
                            where p.IsDelete == false && p.LotteryTime.HasValue && p.LotteryTime > DateTime.Now && p.LotteryTime < lastTime
                            select new WelfareModel
                            {
                                LotteryTime = p.LotteryTime.Value,
                                MarchantName = p.MerchantName,
                                Number = p.Number,
                                PartNumber = p.PartNumber,
                                WelfareID = p.WelfareID,
                                WelfareName = p.Name,
                                WelfareType = p.WelfareType,
                                WinNumber = p.WinNumber,
                                IsDelete = p.IsDelete,
                                ExpiryStartTime = p.ExpiryStartTime,
                                ExpiryEndTime = p.ExpiryEndTime,
                                Status = p.Status
                            };

                return query.OrderBy(p => p.LotteryTime).ToList();
            }
        }

        /// <summary>
        /// 获取福利信息
        /// </summary>
        /// <param name="welfareID"></param>
        /// <returns></returns>
        public static WelfareModel GetWelfare(long welfareID)
        {
            using (var db = new DataContext())
            {
                var query = from p in db.Merchant_Welfare
                            where p.WelfareID == welfareID
                            select new WelfareModel
                            {
                                LotteryTime = p.LotteryTime.Value,
                                MarchantName = p.MerchantName,
                                Number = p.Number,
                                PartNumber = p.PartNumber,
                                WelfareID = p.WelfareID,
                                WelfareName = p.Name,
                                WelfareType = p.WelfareType,
                                WinNumber = p.WinNumber,
                                IsDelete = p.IsDelete,
                                ExpiryStartTime = p.ExpiryStartTime,
                                ExpiryEndTime = p.ExpiryEndTime,
                                Status = p.Status
                            };

                return query.FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取福利活动的所有参与编号
        /// </summary>
        /// <param name="welfareID"></param>
        /// <returns></returns>
        public static string[] GetAllPartCode(long welfareID)
        {
            using (var db = new DataContext())
            {
                return db.Welfare_PartUser.Where(p => p.WelfareID == welfareID).Select(p => p.PartCode).ToArray();
            }
        }

        /// <summary>
        /// 写入开奖结果,并将福利放置用户名下等待用户领取（优惠券自动领取）
        /// </summary>
        /// <param name="welfareID"></param>
        /// <param name="partCodes"></param>
        public static WelfareWinnerContent WriteLotteryResult(long welfareID, string[] partCodes)
        {
            using (var db = new DataContext())
            {
                var welfare = db.Merchant_Welfare.SingleOrDefault(p => p.WelfareID == welfareID);

                //是否应立即自动领取
                bool isNowAward = welfare.WelfareType == (int)WelfareType.Coupon;

                //中奖人员集合
                var winners = new List<Welfare_PartUser>();

                #region 存在参与人员，才会有中奖名单
                if (null != partCodes && partCodes.Length > 0)
                {
                    winners = db.Welfare_PartUser.Where(p => p.WelfareID == welfareID && partCodes.Contains(p.PartCode)).ToList();

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

                //总中奖人数
                welfare.WinNumber += winners.Count;
                //总领取人数
                if (isNowAward)
                {
                    welfare.DrawNumber += winners.Count;
                }

                //全部开奖完成后结束
                welfare.Status = (int)WelfareStatus.Done;

                #endregion

                if (db.SaveChanges() > 0)
                {
                    return new WelfareWinnerContent
                    {
                        LolleryTime = welfare.LotteryTime.Value,
                        MerchantID = welfare.MerchantID,
                        MerchantName = welfare.MerchantName,
                        UserIDs = winners.Select(p => p.UserID).ToArray(),
                        WelfareID = welfare.WelfareID,
                        WelfareName = welfare.Name,
                        WelfareType = welfare.WelfareType
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

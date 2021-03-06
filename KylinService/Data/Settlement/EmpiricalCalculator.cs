﻿using System;
using System.Linq;
using Td.Kylin.DataCache;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 经验值计算器
    /// </summary>
    public sealed class EmpiricalCalculator
    {
        /// <summary>
        /// 初始化经验值计算器实例
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="activityType"></param>
        public EmpiricalCalculator(long userID, UserActivityType activityType)
        {
            this._userID = userID;
            this._activityType = activityType;
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        private long _userID;

        /// <summary>
        /// 业务活动类型
        /// </summary>
        private UserActivityType _activityType;

        private int _score=-1;
        /// <summary>
        /// 业务活动影响的经验值
        /// </summary>
        public int Score
        {
            get
            {
                if (_score < 0)
                {
                    Calc();
                }
                return _score;
            }
        }

        /// <summary>
        /// 是否允许继续奖/罚积分
        /// </summary>
        public bool CanContinue
        {
            get
            {
                return Score > 0;
            }
        }

        /// <summary>
        /// 计算开始
        /// </summary>
        void Calc()
        {
            int score = 0;
            var config = CacheCollection.UserEmpiricalConfigCache.Get((int)UserActivityType.OrderFinish);
            //存在配置
            if (null != config && config.Score > 0)
            {
                score = config.Score;

                //有限制
                if (config.MaxLimit != 0)
                {
                    //限制单位枚举
                    ScoreMaxLimitUnit unit = (ScoreMaxLimitUnit)Enum.Parse(typeof(ScoreMaxLimitUnit), config.MaxUnit.ToString());

                    switch (unit)
                    {
                        case ScoreMaxLimitUnit.Times: break;
                        case ScoreMaxLimitUnit.Day:
                            //获取今天同一业务活动累积的经验值
                            var todayEmpirical = GetEmpiricalToday();
                            if (Math.Abs(todayEmpirical) + Math.Abs(config.Score) > Math.Abs(config.MaxLimit))
                            {
                                score = 0;
                            }
                            break;
                    }
                }
            }
            this._score = score;
        }

        /// <summary>
        /// 获取当天同一业务活动累积的经验值
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        int GetEmpiricalToday()
        {
            using (var db = new DataContext())
            {
                var query = from p in db.User_EmpiricalRecords
                            where p.UserID == _userID && p.ActivityType == (int)_activityType && p.CreateTime >= DateTime.Now.Date
                            select p.Score;

                return query.Sum();
            }
        }
    }
}

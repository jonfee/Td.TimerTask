using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Td.Task.Framework
{
    /// <summary>
    /// 时间计算器
    /// </summary>
    public class TimeCalculator
    {
        /// <summary>
        /// 时间误差（200毫秒）
        /// </summary>
        public static TimeSpan Deviation = new TimeSpan(200000);

        /// <summary>
        /// 策略变更
        /// </summary>
        /// <param name="config"></param>
        public static void CheckStrategy(TimerStrategy config)
        {
            var Config = config;
            if (Config == null || Config.StrategyByDay == null) throw new Exception("定时器时间配置异常！");
            if (Config == null) throw new Exception("定时器时间配置异常！");

            switch (Config.TimerMode)
            {
                case TimerMode.OnTimes:
                    if (Config.OnTimes == null || Config.OnTimes.Count == 0) throw new Exception("定时器时间配置异常，OnTimes（取值不能为空）！");
                    Config.OnTimes = TimerStrategyManager.SortAndDistinct(Config.OnTimes);
                    break;
                case TimerMode.EveryDay:
                    return;
            }
        }

        /// <summary>
        /// 检测时间是否到了
        /// </summary>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static bool TimeIsUp(TimerStrategy strategy)
        {
            DateTime now = DateTime.Now;

            if (CheckTimeIsUp(strategy.StrategyByDay, now.TimeOfDay))
            {
                bool isUp = false;

                switch (strategy.TimerMode)
                {
                    case TimerMode.OnTimes: isUp = strategy.OnTimes.Contains(now); break;
                    case TimerMode.EveryDay: isUp = true; break;
                }

                return isUp;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
		/// 时间是否到了
		/// </summary>
		/// <param name="dayStrategy"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		private static bool CheckTimeIsUp(TimerStrategyByDay dayStrategy, TimeSpan time)
        {
            time = time.Add(Deviation);
            var tmp = new TimeSpan(time.Hours, time.Minutes, time.Seconds);

            if (dayStrategy.TimePoints == null)
                return (tmp.Ticks == 0);
            else
            {
                foreach (var t in dayStrategy.TimePoints)
                {
                    if (t == tmp) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取从现在起到下次执行时间剩余的时间
        /// </summary>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static TimeSpan GetNextTimeUp(TimerStrategy strategy)
        {
            //目标时间
            DateTime nextTime = GetNextDateTime(strategy);

            return nextTime - DateTime.Now;
        }

        /// <summary>
        /// 获取下一次指定配置的时间是多少
        /// </summary>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static DateTime GetNextDateTime(TimerStrategy strategy)
        {
            DateTime dt = DateTime.Now;

            TimeSpan MinTimeByDay;
            bool hasExpectDate;

            TimeSpan time = GetNextTimeConfig(strategy.StrategyByDay, dt, out MinTimeByDay, out hasExpectDate);

            DateTime now, target;

            switch (strategy.TimerMode)
            {
                case TimerMode.OnTimes:
                    dt = GetTargetDateTimeByOnTime(dt, strategy.OnTimes);
                    break;
                case TimerMode.EveryDay:
                    #region (Checked)每天指定某时执行一次
                    now = new DateTime(1, 1, 1, dt.Hour, dt.Minute, dt.Second);
                    target = new DateTime(1, 1, 1, time.Hours, time.Minutes, time.Seconds);
                    if (now.Ticks >= target.Ticks) dt = dt.AddDays(1.0);    //如果当前时间小于指定时刻，则不需要加天

                    dt = new DateTime(dt.Year, dt.Month, dt.Day, time.Hours, time.Minutes, time.Seconds);
                    #endregion
                    break;
            }

            return dt;
        }

        /// <summary>
		/// 获取下一个时间点
		/// </summary>
		/// <param name="dayStrategy">时间策略</param>
		/// <param name="currDate">当前时间</param>
		/// <param name="minData">最小时间</param>
		/// <param name="hasExpectData">是否有找到期望的时间</param>
		/// <returns></returns>
		private static TimeSpan GetNextTimeConfig(TimerStrategyByDay dayStrategy, DateTime currDate, out TimeSpan minData, out bool hasExpectData)
        {
            if (dayStrategy.TimePoints == null || dayStrategy.TimePoints.Length == 0)
                dayStrategy.TimePoints = new TimeSpan[] { new TimeSpan(0) };

            minData = new TimeSpan(23, 59, 59);     // 最小时间
            var minExpectData = TimeSpan.MaxValue;  // 大于当前时间的最小时间
            foreach (var t in dayStrategy.TimePoints)
            {
                if (currDate.TimeOfDay < t && minExpectData >= t)   // 找出比当前时间大的最小时间
                    minExpectData = t;
                if (minData > t)    // 找出最小的一个时间，当前时间不参与运算
                    minData = t;
            }
            hasExpectData = minExpectData != TimeSpan.MaxValue;
            if (hasExpectData)  // 如果找到比当前时间大的最小时间，则返回该时间
                return minExpectData;
            else
                return minData;
        }

        #region 目标时间计算

        /// <summary>
        /// 获取下一个执行日期时间（TimerMode.OnTime）
        /// </summary>
        /// <param name="currDate"></param>
        /// <param name="Times"></param>
        /// <returns></returns>
        private static DateTime GetTargetDateTimeByOnTime(DateTime currDate, List<DateTime> Times)
        {
            foreach (var dt in Times)
            {
                if (dt > currDate)
                {
                    return dt;
                }
            }

            return DateTime.MinValue;
        }

        #endregion
    }
}

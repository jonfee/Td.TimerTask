using System;

namespace Td.Task.Framework
{
    /// <summary>
	/// 每天的时间策略
	/// </summary>
	public class TimerStrategyByDay
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public TimeSpan BeginTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public TimeSpan EndTime { get; set; }
        /// <summary>
        /// 间隔时间
        /// </summary>
        public TimeSpan Interval { get; set; }
        /// <summary>
        /// 时间点
        /// </summary>
        public TimeSpan[] TimePoints { get; set; }
    }
}

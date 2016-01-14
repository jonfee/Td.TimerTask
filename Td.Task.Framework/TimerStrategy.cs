using System;
using System.Collections.Generic;

namespace Td.Task.Framework
{
    /// <summary>
    /// 定时任务策略
    /// </summary>
    public class TimerStrategy
    {
        /// <summary>
		/// 配置引用名
		/// </summary>
		public string RefName { get; set; }

        /// <summary>
        /// 是否可重入
        /// </summary>
        public bool ReEntrant { get; set; }

        /// <summary>
        /// 时间模式
        /// </summary>
        public TimerMode TimerMode { get; set; }

        /// <summary>
        /// 指定的执行时间集合
        /// </summary>
        public List<DateTime> OnTimes { get; set; }

        /// <summary>
        /// 循环处理时间间隔（单位活动）
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// 启动延迟时间（单位：毫秒）
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// 每天的时间配置
        /// </summary>
        public TimerStrategyByDay StrategyByDay { get; set; }
    }
}

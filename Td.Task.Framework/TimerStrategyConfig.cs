namespace Td.Task.Framework
{
    /// <summary>
    /// 策略配置
    /// </summary>
    public class TimerStrategyConfig
    {
        /// <summary>
		/// 默认配置
		/// </summary>
		public string Default { get; set; }

        /// <summary>
        /// 配置项
        /// </summary>
        public TimerStrategy[] Config { get; set; }
    }
}

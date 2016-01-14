namespace Td.Task.Framework
{
    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TimerTaskStatus
    {
        /// <summary>
        /// 准备就绪
        /// </summary>
        Standby,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
        /// <summary>
        /// 完成
        /// </summary>
        Completed,
        /// <summary>
        /// 停止
        /// </summary>
        Stopped,
        /// <summary>
        /// 异常终止
        /// </summary>
        Exception
    }

    /// <summary>
    /// 策略模式
    /// </summary>
    public enum TimerMode
    {
        /// <summary>
        /// 轮询方式
        /// </summary>
        Interval,
        /// <summary>
        /// 指定时间执行（可多时间）
        /// </summary>
        OnTimes,
        ///// <summary>
        ///// 哪些月的哪些天数的哪些时间可以执行
        ///// </summary>
        //Month,
        ///// <summary>
        ///// 每周中的哪些星期的哪些时间可以执行
        ///// </summary>
        //EveryWeek,
        /// <summary>
        /// 每天中的哪些时间可以执行
        /// </summary>
        EveryDay,
        ///// <summary>
        ///// 每个月指定的哪些天的哪些时间可以执行
        ///// </summary>
        //EveryMonth,
        ///// <summary>
        ///// 每年中哪些天的哪些时间可以执行
        ///// </summary>
        //EveryYear,
        ///// <summary>
        ///// 每年中哪些指定日期的哪些时间可以执行
        ///// </summary>
        //Date,
        ///// <summary>
        ///// 每个月倒数的哪些天的哪些时间可以执行
        ///// </summary>
        //LastDayOfMonth
    }
}

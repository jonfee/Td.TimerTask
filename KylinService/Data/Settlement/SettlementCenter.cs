namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 结算中心抽象类
    /// </summary>
    public abstract class SettlementCenter
    {
        /// <summary>
        /// 是否结算成功
        /// </summary>
        public abstract bool Success { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public abstract string ErrorMessage { get; }

        /// <summary>
        /// 执行结算
        /// </summary>
        public abstract void Execute();
    }
}

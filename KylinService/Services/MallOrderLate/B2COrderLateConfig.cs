namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 订单延期相关配置项
    /// </summary>
    public class B2COrderLateConfig
    {
        /// <summary>
        /// 下单后等待支付的时间（单位：分钟）
        /// </summary>
        public int WaitPaymentMinutes { get; set; }

        /// <summary>
        /// 发货后等待收货的时间（单位：天）
        /// </summary>
        public int WaitReceiptGoodsDays { get; set; }

        /// <summary>
        /// 等待用户评价的时间（单位：天）
        /// </summary>
        public int WaitEvaluateDays { get; set; }
    }
}

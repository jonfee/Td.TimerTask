namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 订单延期相关配置项
    /// </summary>
    public class OrderLateConfig
    {
        /// <summary>
        /// 下单后等待支付的时间（单位：小时）
        /// </summary>
        public int WaitPaymentHours { get; set; }

        /// <summary>
        /// 发货后等待收货的时间（单位：天）
        /// </summary>
        public int WaitReceiptGoodsDays { get; set; }
    }
}

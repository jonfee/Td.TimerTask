namespace KylinService.SysEnums
{
    /// <summary>
    /// 上门预约订单支付方式
    /// </summary>
    public enum AppointPaymentType
    {
        /// <summary>
        /// 线下付款
        /// </summary>
        OffLine = 1,
        /// <summary>
        /// 余额支付
        /// </summary>
        Balance = 2,
        /// <summary>
        /// 微信支付
        /// </summary>
        Weixin = 4,
        /// <summary>
        /// 支付宝支付
        /// </summary>
        Alipay = 8
    }
}

namespace KylinService.SysEnums
{
    /// <summary>
    /// 上门预约订单超时类型
    /// </summary>
    public enum AppointLateType
    {
        /// <summary>
        /// 超时未支付
        /// </summary>
        LateNoPayment=1,
        /// <summary>
        /// 用户超时未确认服务完成
        /// </summary>
        LateUserFinish=2
    }
}

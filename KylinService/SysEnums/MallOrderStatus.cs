namespace KylinService.SysEnums
{
    /// <summary>
    /// 商城订单状态
    /// </summary>
    public enum MallOrderStatus
    {
        /// <summary>
        /// 未付款
        /// </summary>
        NoPay = 1,
        /// <summary>
        /// 待发货
        /// </summary>
        WaitSendGoods = 2,
        /// <summary>
        /// 待收货
        /// </summary>
        WaitReceiptGoods = 4,
        /// <summary>
        /// 已完成
        /// </summary>
        Finish = 8,
        /// <summary>
        /// 已取消
        /// </summary>
        Cancel = 16
    }

}

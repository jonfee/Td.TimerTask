namespace KylinService.SysEnums
{
    /// <summary>
    /// 订单业务支付方类型（由谁来付钱）
    /// </summary>
    public enum BusinessPayerType
    {
        /// <summary>
        /// 下单方（指用户）
        /// </summary>
        OrderCreator=1,
        /// <summary>
        /// 服务方（指商家或个人服务者）
        /// </summary>
        Server=2
    }
}

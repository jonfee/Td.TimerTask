namespace KylinService.SysEnums
{
    /// <summary>
    /// 用户交易类型
    /// </summary>
    public enum UserTradType
    {
        /// <summary>
        /// 余额充值
        /// </summary>
        Recharge = 1,
        /// <summary>
        /// 购买商品
        /// </summary>
        BuyProduct = 2,
        /// <summary>
        /// 支付服务费用
        /// </summary>
        PayService = 4,
        /// <summary>
        /// 冲退回款
        /// </summary>
        PaymentBack = 8,
        /// <summary>
        /// 提现
        /// </summary>
        Withdraw = 16,
        /// <summary>
        /// 零售物品所得
        /// </summary>
        SaleGet = 32
    }
}

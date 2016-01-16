namespace KylinService.SysEnums
{
    /// <summary>
    /// 商家交易类型
    /// </summary>
    public enum MerchantTradType
    {
        /// <summary>
        /// 商品销售
        /// </summary>
        SaleProduct = 1,
        /// <summary>
        /// 服务销售
        /// </summary>
        SaleService = 2,
        /// <summary>
        /// 支付服务佣金
        /// </summary>
        PayToWorker = 4,
        /// <summary>
        /// 冲退回款
        /// </summary>
        PaymentBack = 8,
        /// <summary>
        /// 提现
        /// </summary>
        Withdraw = 16

    }
}

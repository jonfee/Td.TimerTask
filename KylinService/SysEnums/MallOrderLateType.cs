﻿namespace KylinService.SysEnums
{
    /// <summary>
    /// 商城订单超时类型
    /// </summary>
    public enum MallOrderLateType
    {
        /// <summary>
        /// 超时未支付
        /// </summary>
        LateNoPayment=1,
        /// <summary>
        /// 用户超时未确认收货
        /// </summary>
        LateUserFinish=2,
        /// <summary>
        /// 超时未评价
        /// </summary>
        LateNoEvaluate=4
    }
}

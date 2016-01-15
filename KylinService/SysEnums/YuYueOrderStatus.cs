namespace KylinService.SysEnums
{
    /// <summary>
    /// 预约订单状态
    /// </summary>
    public enum YuYueOrderStatus
    {
        /// <summary>
        /// 等待商家接单
        /// </summary>
        WaitReceiving = 1,
        /// <summary>
        /// 已接单，正在安排服务
        /// </summary>
        Received = 2,
        /// <summary>
        /// 商家提交服务方案
        /// </summary>
        SubmitStudio = 4,
        /// <summary>
        /// 用户确定服务方案
        /// </summary>
        ConfirmStudio = 8,
        /// <summary>
        /// 工作人员结束服务
        /// </summary>
        WorkerFinish = 16,
        /// <summary>
        /// 用户结束服务
        /// </summary>
        UserFinish = 32,
        /// <summary>
        /// 用户已评价
        /// </summary>
        HaveEvaluation = 64,
        /// <summary>
        /// 订单已取消
        /// </summary>
        Cancel = 128
    }
}

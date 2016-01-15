namespace KylinService.SysEnums
{
    /// <summary>
    /// 上门订单状态
    /// </summary>
    public enum ShangMenOrderStatus
    {
        /// <summary>
        /// 等待商家接单
        /// </summary>
        WaitReceiving = 1,
        /// <summary>
        /// 已接单，正在安排工作人员
        /// </summary>
        Received = 2,
        /// <summary>
        /// 等待工作人员上门
        /// </summary>
        WaitWorkerComeOn = 4,
        /// <summary>
        /// 工作人员开始工作
        /// </summary>
        Working = 8,
        /// <summary>
        /// 工作人员已报价
        /// </summary>
        SubmitStudio = 16,
        /// <summary>
        /// 用户确定订单
        /// </summary>
        ConfirmStudio = 32,
        /// <summary>
        /// 工作人员结束服务
        /// </summary>
        WorkerFinish = 64,
        /// <summary>
        /// 用户结束服务
        /// </summary>
        UserFinish = 128,
        /// <summary>
        /// 用户已评价
        /// </summary>
        HaveEvaluation = 256,
        /// <summary>
        /// 订单已取消
        /// </summary>
        Cancel = 512
    }
}

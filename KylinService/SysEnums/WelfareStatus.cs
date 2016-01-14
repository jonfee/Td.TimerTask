namespace KylinService.SysEnums
{
    /// <summary>
    /// 福利状态枚举
    /// </summary>
    public enum WelfareStatus
    {
        /// <summary>
        /// 审核中
        /// </summary>
        Auditing = 1,
        /// <summary>
        /// 审核失败
        /// </summary>
        AuditFailure = 2,
        /// <summary>
        /// 未排期（已审核通过，但未安排发放时间）
        /// </summary>
        SuccessNoPhases = 4,
        /// <summary>
        /// 进行中（已安排发放）
        /// </summary>
        SuccessProgresse = 8,
        /// <summary>
        /// 已结束（所有福利排期均已结束）
        /// </summary>
        Finish = 16,
        /// <summary>
        /// 已关闭（人为关闭福利）
        /// </summary>
        Close = 32

    }
}

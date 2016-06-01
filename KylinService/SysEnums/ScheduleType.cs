using System.ComponentModel;

namespace KylinService.SysEnums
{
    /// <summary>
    /// 队列计划任务类型
    /// </summary>
    public enum QueueScheduleType
    {
        /// <summary>
        /// 社区活动提醒
        /// </summary>
        [Description("社区活动提醒")]
        CircleEventRemind,
        /// <summary>
        /// 福利报名提醒
        /// </summary>
        [Description("福利报名提醒")]
        WelfareBaoMinRemind,
        /// <summary>
        /// 福利开奖
        /// </summary>
        [Description("福利开奖")]
        WelfareLottery,
        /// <summary>
        /// 精品汇超时未付款
        /// </summary>
        [Description("精品汇超时未付款")]
        MallOrderLatePayment,
        /// <summary>
        /// 精品汇超时未收货
        /// </summary>
        [Description("精品汇超时未收货")]
        MallOrderLateReceive,
        /// <summary>
        /// 附近购超时未付款
        /// </summary>
        [Description("附近购超时未付款")]
        MerchantOrderLatePayment,
        /// <summary>
        /// 附近购超时未收货
        /// </summary>
        [Description("附近购超时未收货")]
        MerchantOrderLateReceive,
        /// <summary>
        /// 上门订单超时未付款
        /// </summary>
        [Description("上门订单超时未付款")]
        VisitingOrderLatePayment,
        /// <summary>
        /// 上门订单超时未确认完成
        /// </summary>
        [Description("上门订单超时未确认完成")]
        VisitingOrderLateConfirmDone,
        /// <summary>
        /// 预约订单超时未付款
        /// </summary>
        [Description("预约订单超时未付款")]
        ReservationOrderLatePayment,
        /// <summary>
        /// 上门订单超时未确认完成
        /// </summary>
        [Description("预约订单超时未确认完成")]
        ReservationOrderLateConfirmDone,
        /// <summary>
        /// 跑腿业务员接单超时时间
        /// </summary>
        [Description("跑腿业务员接单超时时间")]
        LegworkOrderTimeout,
        /// <summary>
        /// 跑腿业务员支付超时时间
        /// </summary>
        [Description("跑腿业务员支付超时时间")]
        LegworkPaymentTimeout,
        /// <summary>
        /// 跑腿业务员自动确认收货时间
        /// </summary>
        [Description("跑腿业务员自动确认收货时间")]
        LegworkAutoConfirmTime
    }

    /// <summary>
    /// 清理计划任务类型
    /// </summary>
    public enum ClearScheduleType
    {
        /// <summary>
        /// 摇一摇每日清理
        /// </summary>
        [Description("摇一摇每日清理")]
        ShakeDayTimesClear
    }
}

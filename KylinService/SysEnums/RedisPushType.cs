using System.ComponentModel;

namespace KylinService.SysEnums
{
    /// <summary>
    /// Redis存储类型
    /// </summary>
    public enum RedisPushType
    {
        /// <summary>
        /// 限时福利中奖消息
        /// </summary>
        [Description("限时福利中奖消息")]
        WelfareLottery = 1,
        /// <summary>
        /// 社区活动开始提醒
        /// </summary>
        [Description("社区活动开始提醒")]
        CircleEventRemind = 2,
        /// <summary>
        /// 福利报名提醒
        /// </summary>
        [Description("福利报名提醒")]
        WelfareRemind = 3,
        /// <summary>
        /// 精品汇订单确认收货消息
        /// </summary>
        [Description("精品汇订单确认收货消息")]
        B2COrderReceivedGoods = 4,
        /// <summary>
        /// 附近购订单确认收货消息
        /// </summary>
        [Description("附近购订单确认收货消息")]
        MerchantOrderReceivedGoods = 5
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KylinService.SysEnums
{
    /// <summary>
    /// 任务计划类型
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// 限时福利开奖
        /// </summary>
        [Description("限时福利开奖服务")]
        WelfareLottery,
        /// <summary>
        /// 精品汇订单逾期处理
        /// </summary>
        [Description("精品汇订单逾期处理服务")]
        MallOrderLate,
        /// <summary>
        /// 附近购订单逾期处理
        /// </summary>
        [Description("附近购订单逾期处理")]
        MerchantOrderLate,
        /// <summary>
        /// 上门预约服务订单逾期处理
        /// </summary>
        [Description("上门预约服务订单逾期处理")]
        AppointOrderLate,
        /// <summary>
        /// 摇一摇每日次数清除服务
        /// </summary>
        [Description("摇一摇每日次数清除服务")]
        ShakeDayTimesClear
    }
}

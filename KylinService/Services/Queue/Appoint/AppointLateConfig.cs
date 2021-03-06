﻿namespace KylinService.Services.Queue.Appoint
{
    /// <summary>
    /// 上门预约逾期处理时配置
    /// </summary>
    public class AppointLateConfig
    {
        /// <summary>
        /// 下单后多少分钟必须支付（指必须先支付的订单）
        /// </summary>
        public int PaymentWaitMinutes { get; set; }

        /// <summary>
        /// 服务人员结束服务后用户可确认的时间天数
        /// </summary>
        public int EndServiceWaitUserDays { get; set; }

        /// <summary>
        /// 用户可评价时间的天数
        /// </summary>
        public int EvaluateWaitDyas { get; set; }
    }
}

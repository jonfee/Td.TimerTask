namespace KylinService.Services.Appoint
{
    /// <summary>
    /// 上门预约逾期处理时配置
    /// </summary>
    public class AppointConfig
    {
        /// <summary>
        /// 下单后多少分钟必须支付（指必须先支付的订单）
        /// </summary>
        public int PaymentWaitMinutes { get; set; }

        /// <summary>
        /// 服务人员结束服务后用户可确认的时间天数
        /// </summary>
        public int EndServiceWaitUserDays { get; set; }
    }
}

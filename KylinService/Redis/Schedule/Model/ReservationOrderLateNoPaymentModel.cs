using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 预约未支付订单
    /// </summary>
    public class ReservationOrderLateNoPaymentModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderID { get; set; }

        /// <summary>
        /// 最后支付的期限
        /// </summary>
        public DateTime LastPaymentTime { get; set; }
    }
}

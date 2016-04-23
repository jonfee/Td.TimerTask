using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 附近购未支付订单
    /// </summary>
    public class MerchantOrderLateNoPaymentModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderID { get; set; }

        ///<summary>
        ///下单时间
        ///</summary>

        public DateTime CreateTime { get; set; }
    }
}

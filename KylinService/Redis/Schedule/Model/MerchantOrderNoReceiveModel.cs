using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 附近购已发货的订单
    /// </summary>
    public class MerchantOrderNoReceiveModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderID { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}

using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 预约未确定完成的订单
    /// </summary>
    public class ReservationOrderLateReceiveModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderID { get; set; }
        
        /// <summary>
        /// 服务职员结束服务时间
        /// </summary>
        public DateTime WorkerFinishTime { get; set; }
    }
}

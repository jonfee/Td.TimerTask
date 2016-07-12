using KylinService.Services;
using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 上门未确定完成的订单
    /// </summary>
    public class VisitingOrderLateReceiveModel:ServiceState
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

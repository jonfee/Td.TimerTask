using KylinService.Services;
using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 精品汇已发货的订单
    /// </summary>
    public class MallOrderLateReceiveModel:ServiceState
    {
        ///<summary>
        ///订单ID
        ///</summary>
        public long OrderID { get; set; }

        ///<summary>
        ///发货时间
        ///</summary>

        public DateTime ShipTime { get; set; }
    }
}

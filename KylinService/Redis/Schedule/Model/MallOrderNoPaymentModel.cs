using KylinService.Services;
using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 精品汇未支付订单
    /// </summary>
    public class MallOrderNoPaymentModel : ServiceState
    {
        ///<summary>
        ///订单ID
        ///</summary>
        public long OrderID { get; set; }

        ///<summary>
        ///下单时间
        ///</summary>

        public DateTime CreateTime { get; set; }

        ///<summary>
        ///必须支付的时间
        ///</summary>

        public DateTime? NeedPayTime { get; set; }
    }
}

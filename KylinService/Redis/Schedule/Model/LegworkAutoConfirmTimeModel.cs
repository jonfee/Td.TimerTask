using KylinService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KylinService.Redis.Schedule.Model
{
    public class LegworkAutoConfirmTimeModel : ServiceState
    {
        public long OrderID
        {
            get; set;
        }
        /// <summary>
        /// 订单送达时间
        /// </summary>
        public DateTime? ActualDeliveryTime
        {
            get;
            set;
        }
    }
}

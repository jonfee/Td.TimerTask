using KylinService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KylinService.Redis.Schedule.Model
{
    public class LegworkPaymentTimeoutModel : ServiceState
    {
        public long OrderID
        {
            get; set;
        }
        /// <summary>
        /// 订单报价时间
        /// </summary>
        public DateTime? OfferAcceptTime
        {
            get;
            set;
        }
    }
}

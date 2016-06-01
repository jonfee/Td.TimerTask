using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KylinService.Services.Queue.Legwork.Model
{
    public class PaymentTimeoutModel
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

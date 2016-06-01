using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KylinService.Data.Model
{
    public class LegworkOrderModel
    {
        public long OrderID
        {
            get;
            set;
        }
        public DateTime SubmitTime
        {
            get;
            set;
        }
        public DateTime? ActualDeliveryTime
        {
            get;
            set;
        }
        public DateTime? OfferAcceptTime
        {
            get;
            set;
        }
        public short Status
        {
            get;
            set;
        }
        public  string OrderCode
        {
            get;
            set;
        }
    }
}

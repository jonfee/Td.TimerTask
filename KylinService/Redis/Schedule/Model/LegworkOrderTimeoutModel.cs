using KylinService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KylinService.Redis.Schedule.Model
{
    public class LegworkOrderTimeoutModel: ServiceState
    {
        public long OrderID
        {
            get; set;
        }
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KylinService.Services.Queue.Legwork.Model
{
    public class OrderTimeoutModel
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

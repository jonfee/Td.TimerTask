using KylinService.Core;
using KylinService.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KylinService.Services.Appoint
{
    /// <summary>
    /// 上门预约订单超时自动处理任务计划基类
    /// </summary>
    public class BaseAppointOrderLateScheduler : IScheduler
    {
        public BaseAppointOrderLateScheduler(AppointConfig config, AppointOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.Order = order;

            this.Config = config;

            this.Key = null != this.Order ? this.Order.OrderID.ToString() : string.Empty;
        }

        /// <summary>
        /// 商城订单
        /// </summary>
        public AppointOrderModel Order { get; private set; }

        /// <summary>
        /// 逾期配置
        /// </summary>
        public AppointConfig Config { get; private set; }

        /// <summary>
        /// 定时器
        /// </summary>
        public System.Threading.Timer LateTimer { get; protected set; }

        protected override void Execute()
        {
            //执行
        }
    }
}

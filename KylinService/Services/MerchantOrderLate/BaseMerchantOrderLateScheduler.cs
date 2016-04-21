using KylinService.Core;
using KylinService.Data.Model;
using System.Windows.Forms;

namespace KylinService.Services.MerchantOrderLate
{
    /// <summary>
    /// 附近购订单超时自动处理任务计划基类
    /// </summary>
    public class BaseMerchantOrderLateScheduler : IScheduler
    {
        /// <summary>
        /// 商城订单
        /// </summary>
        public MerchantOrderModel Order { get; private set; }

        /// <summary>
        /// 逾期配置
        /// </summary>
        public MerchantOrderLateConfig Config { get; private set; }

        /// <summary>
        /// 定时器
        /// </summary>
        public System.Threading.Timer LateTimer { get; protected set; }

        public BaseMerchantOrderLateScheduler(MerchantOrderLateConfig config, MerchantOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.Order = order;

            this.Config = config;

            this.Key = null != this.Order ? this.Order.OrderID.ToString() : string.Empty;
        }

        protected override void Execute()
        {
            //
        }
    }
}

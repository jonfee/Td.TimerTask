using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using System;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;

namespace KylinService.Services.MerchantOrderLate
{
    /// <summary>
    /// 商家订单逾期未付款自动取消订单任务计划
    /// </summary>
    public class MerchantOrderPaymentLateScheduler : BaseMerchantOrderLateScheduler
    {
        public MerchantOrderPaymentLateScheduler(MerchantOrderLateConfig config, MerchantOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(config, order, form, writeDelegate)
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                DateTime baseTime = MerchantOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MerchantOrderLateType.LateNoPayment);

                //计算自动取消订单时间的时间差
                TimeSpan dueTime = baseTime - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                LateTimer = new System.Threading.Timer((obj) =>
                {
                    //计划执行
                    this.Start();
                    
                    //释放定时器
                    LateTimer.Dispose();

                }, null, (int)Math.Ceiling(dueTime.TotalMilliseconds), Timeout.Infinite);
            }
        }

        protected override void Execute()
        {
            if (null == Order) return;

            try {
                var lastOrder = MerchantOrderProvider.GetOrder(Order.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (!CheckAutoOk(lastOrder)) throw new Exception("当前订单状态发生变更，不能自动取消订单");

                var lastTimeout = MerchantOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MerchantOrderLateType.LateNoPayment);

                if (DateTime.Now < lastTimeout) throw new Exception("支付期限未到，不能自动取消订单！");

                //自动取消订单
                bool cancelSuccess = MerchantOrderProvider.AutoCancelOrder(Order.OrderID).Result;

                string message = string.Empty;

                if (cancelSuccess)
                {
                    message = string.Format("〖订单（{0}）〗因超时未付款，系统已自动取消订单！", Order.OrderCode);
                }
                else
                {
                    message = string.Format("〖订单（{0}）〗因超时未付款，系统自动取消订单时操作失败！", Order.OrderCode);
                }

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("〖订单（{0}）〗自动取消订单失败，原因：{1}", Order.OrderCode, ex.Message);
                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, errMsg);
            }
        }

        /// <summary>
        /// 检测自动处理的订单有效性
        /// </summary>
        /// <returns></returns>
        private bool CheckAutoOk(MerchantOrderModel order)
        {
            return order.OrderStatus == (int)MerchantOrderStatus.WaitingPayment;
        }
    }
}

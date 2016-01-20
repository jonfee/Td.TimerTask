using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 商城订单逾期未付款自动取消订单任务计划
    /// </summary>
    public class MallOrderPaymentLateScheduler : BaseMallOrderLateScheduler
    {
        public MallOrderPaymentLateScheduler(OrderLateConfig config, MallOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(config, order, form, writeDelegate)
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                DateTime baseTime = MallOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MallOrderLateType.LateNoPayment);

                //计算自动取消订单时间的时间差
                TimeSpan dueTime = baseTime - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                LateTimer = new System.Threading.Timer((obj) =>
                {
                    //计划执行
                    this.Start();

                    LateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    LateTimer.Dispose();
                    LateTimer = null;

                }, null, dueTime, dueTime);
            }
        }

        protected override void Execute()
        {
            if (null == Order) return;

            var lastOrder = MallOrderProvider.GetOrder(Order.OrderID);

            if (null == lastOrder) throw new Exception("订单信息已不存在！");

            if (!CheckAutoOk(lastOrder)) throw new Exception("当前订单状态发生变更，不能自动取消订单");

            var lastTimeout = MallOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MallOrderLateType.LateNoPayment);

            if (DateTime.Now < lastTimeout) throw new Exception("支付期限未到，不能自动取消订单！");

            //自动取消订单
            bool cancelSuccess = MallOrderProvider.AutoCancelOrder(Order.OrderID);

            if (cancelSuccess)
            {
                string cancelSuccMessage = string.Format("〖订单（{0}}）：{1}〗因超时未付款，系统已自动取消订单！", Order.OrderCode, Order.ProductInfo);

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, cancelSuccMessage);
            }
            else
            {
                string cancelFailMessage = string.Format("〖订单（{0}}）：{1}〗因超时未付款，系统自动取消订单时操作失败！", Order.OrderCode, Order.ProductInfo);

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, cancelFailMessage);
            }
        }

        /// <summary>
        /// 检测自动处理的订单有效性
        /// </summary>
        /// <returns></returns>
        private bool CheckAutoOk(MallOrderModel order)
        {
            return order.OrderStatus == (int)SysEnums.MallOrderStatus.NoPay;
        }
    }
}

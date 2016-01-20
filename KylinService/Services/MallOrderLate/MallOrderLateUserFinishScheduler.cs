using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 商城订单逾期未收货自动处理任务计划
    /// </summary>
    public class MallOrderLateUserFinishScheduler : BaseMallOrderLateScheduler
    {
        public MallOrderLateUserFinishScheduler(OrderLateConfig config, MallOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(config, order, form, writeDelegate)
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                DateTime baseTime = MallOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MallOrderLateType.LateUserFinish);

                //计算离开奖时间的时间差
                TimeSpan dueTime = baseTime - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                LateTimer = new System.Threading.Timer((obj) =>
                {
                    //计划执行（开奖）
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

            try
            {
                var lastOrder = MallOrderProvider.GetOrder(Order.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (!CheckAutoOk(lastOrder)) throw new Exception("订单状态已发生变更，不能自动完成收货！");

                var lastTimeout = MallOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MallOrderLateType.LateUserFinish);

                if (DateTime.Now < lastTimeout) throw new Exception("确认收货期限未到，不能自动完成收货！");

                //自动收货确认
                bool receiptSuccess = MallOrderProvider.AutoReceiptGoods(Order.OrderID);

                string message = string.Empty;

                if (receiptSuccess)
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未确认收货，系统已自动收货确认处理！", Order.OrderCode, Order.ProductInfo);
                }
                else
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未确认收货，系统自动收货确认处理时操作失败！", Order.OrderCode, Order.ProductInfo);
                }

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("〖订单（{0}）：{1}〗自动完成收货失败，原因：{2}", Order.OrderCode, Order.ProductInfo, ex.Message);
                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, errMsg);
            }
        }

        /// <summary>
        /// 检测自动处理的订单有效性
        /// </summary>
        /// <returns></returns>
        private bool CheckAutoOk(MallOrderModel order)
        {
            return order.OrderStatus == (int)SysEnums.MallOrderStatus.WaitReceiptGoods;
        }
    }
}

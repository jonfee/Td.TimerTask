using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using KylinService.SysEnums;
using System;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;

namespace KylinService.Services.Appoint
{
    /// <summary>
    /// 上门预约订单逾期支付自动取消订单任务计划
    /// </summary>
    public class AppointOrderPaymentLateScheduler : BaseAppointOrderLateScheduler
    {
        public AppointOrderPaymentLateScheduler(AppointConfig config, AppointOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(config, order, form, writeDelegate)
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                DateTime baseTime = AppointOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.AppointLateType.LateNoPayment);

                //计算自动取消订单时间的时间差
                TimeSpan dueTime = baseTime - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                LateTimer = new System.Threading.Timer((obj) =>
                {
                    //计划执行
                    this.Start();
                    
                    LateTimer.Dispose();
                    LateTimer = null;

                }, null, (int)Math.Ceiling(dueTime.TotalMilliseconds), Timeout.Infinite);
            }
        }

        /// <summary>
        /// 执行自动取消订单
        /// </summary>
        protected override void Execute()
        {
            if (null == Order) return;

            try {

                var lastOrder = AppointOrderProvider.GetAppointOrder(Order.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (!CheckAutoOk(lastOrder)) throw new Exception("订单状态已变更，不能自动取消订单！");

                var lastTimeout = AppointOrderTimeCalculator.GetTimeoutTime(lastOrder, Config, SysEnums.AppointLateType.LateNoPayment);

                if (DateTime.Now < lastTimeout) throw new Exception("支付期限未到，不能自动取消订单！");

                //自动取消订单
                bool success = AppointOrderProvider.AutoCancelOrder(Order.OrderID);

                string message = string.Empty;

                if (success)
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未付款，系统已自动取消订单！", Order.OrderCode, Order.BusinessName);
                }
                else
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未付款，系统自动取消订单时操作失败！", Order.OrderCode, Order.BusinessName);
                }

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("〖订单（{0}）：{1}〗自动取消订单失败，原因：{2}", Order.OrderCode, Order.BusinessName, ex.Message);
                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, errMsg);
            }
        }

        /// <summary>
        /// 检测自动取消订单是否被允许
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool CheckAutoOk(AppointOrderModel order)
        {
            if (order.PaiedTime.HasValue) return false;

            if (order.QuoteWays == (int)BusinessServiceQuote.WhenOrder)
            {
                if (order.BusinessType == (int)BusinessServiceType.Visiting && order.Status == (int)VisitingServiceOrderStatus.WaitingMerchantReceive)//上门订单等待商家接单
                    return true;
                else if (order.BusinessType == (int)BusinessServiceType.Reservation && order.Status == (int)ReservationServiceOrderStatus.WaitingMerchantReceive)//预约订单与状态匹配
                    return true;
            }
            else if (order.QuoteWays == (int)BusinessServiceQuote.WhenMeeting)
            {
                if (order.BusinessType == (int)BusinessServiceType.Visiting && order.Status == (int)VisitingServiceOrderStatus.UserConfirmQuote)//上门订单与状态匹配
                    return true;
                else if (order.BusinessType == (int)BusinessServiceType.Reservation && order.Status == (int)ReservationServiceOrderStatus.UserConfirmSolution)//预约订单与状态匹配
                    return true;
            }

            return false;
        }
    }
}

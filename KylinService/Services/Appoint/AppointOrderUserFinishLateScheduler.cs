using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using KylinService.SysEnums;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.Appoint
{
    /// <summary>
    /// 上门预约订单用户确认服务完成超时自动处理任务计划
    /// </summary>
    public class AppointOrderUserFinishLateScheduler : BaseAppointOrderLateScheduler
    {
        public AppointOrderUserFinishLateScheduler(AppointConfig config, AppointOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(config, order, form, writeDelegate)
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                DateTime baseTime = AppointOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.AppointLateType.LateUserFinish);

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

            try {
                var lastOrder = AppointOrderProvider.GetAppointOrder(Order.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (!CheckAutoOk(lastOrder)) throw new Exception("订单状态已变更，不能自动确认服务完成！");

                var lastTimeout = AppointOrderTimeCalculator.GetTimeoutTime(lastOrder, Config, SysEnums.AppointLateType.LateUserFinish);

                if (DateTime.Now < lastTimeout) throw new Exception("服务完成确认期限未到，不能自动确认服务完成！");

                //自动确认服务完成
                bool success = AppointOrderProvider.AutoFinishByUser(Order.OrderID);

                string message = string.Empty;

                if (success)
                {
                    message = string.Format("〖订单（{0}}）：{1}〗因超时未确认服务完成，系统已自动确认服务完成！", Order.OrderCode, Order.BusinessName);
                }
                else
                {
                    message = string.Format("〖订单（{0}}）：{1}〗因超时未确认服务完成，系统自动确认服务完成时操作失败！", Order.OrderCode, Order.BusinessName);
                }

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("〖订单（{0}}）：{1}〗自动确认服务完成失败，原因：{2}", Order.OrderCode, Order.BusinessName, ex.Message);
                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, errMsg);
            }
        }

        /// <summary>
        /// 检测是否允许自动确认服务完成
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool CheckAutoOk(AppointOrderModel order)
        {
            if (!order.WorkerFinishTime.HasValue) return false;

            if (order.BusinessType == (int)AppointBusinessType.ShangMen && order.Status == (int)ShangMenOrderStatus.WorkerFinish)//上门服务且状态为服务人员已完成服务
                return true;

            if (order.BusinessType == (int)AppointBusinessType.YuYue && order.Status == (int)YuYueOrderStatus.WorkerFinish)//预约服务且状态为商家已完成服务
                return true;

            return false;
        }
    }
}

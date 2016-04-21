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
    /// 商城订单逾期未收货自动处理任务计划
    /// </summary>
    public class MerchantOrderLateUserFinishScheduler : BaseMerchantOrderLateScheduler
    {
        public MerchantOrderLateUserFinishScheduler(MerchantOrderLateConfig config, MerchantOrderModel order, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(config, order, form, writeDelegate)
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                DateTime baseTime = MerchantOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MerchantOrderLateType.LateUserFinish);

                //计算离开奖时间的时间差
                TimeSpan dueTime = baseTime - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                LateTimer = new System.Threading.Timer((obj) =>
                {
                    //计划执行
                    this.Start();
                    
                    LateTimer.Dispose();

                }, null, (int)Math.Ceiling(dueTime.TotalMilliseconds), Timeout.Infinite);
            }
        }

        protected override void Execute()
        {
            if (null == Order) return;

            try
            {
                var lastOrder = MerchantOrderProvider.GetOrder(Order.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (!CheckAutoOk(lastOrder)) throw new Exception("订单状态已发生变更，不能自动完成收货！");

                var lastTimeout = MerchantOrderTimeCalculator.GetTimeoutTime(Order, Config, SysEnums.MerchantOrderLateType.LateUserFinish);

                if (DateTime.Now < lastTimeout) throw new Exception("确认收货期限未到，不能自动完成收货！");

                //自动收货确认
                bool receiptSuccess = MerchantOrderProvider.AutoReceiptGoods(Order.OrderID).Result;

                string message = string.Empty;

                if (receiptSuccess)
                {
                    message = string.Format("〖订单（{0}）〗因超时未确认收货，系统已自动收货确认处理！", Order.OrderCode);
                }
                else
                {
                    message = string.Format("〖订单（{0}）〗因超时未确认收货，系统自动收货确认处理时操作失败！", Order.OrderCode);
                }

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("〖订单（{0}）〗自动完成收货失败，原因：{2}", Order.OrderCode, ex.Message);
                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, errMsg);
            }
        }

        /// <summary>
        /// 检测自动处理的订单有效性
        /// </summary>
        /// <returns></returns>
        private bool CheckAutoOk(MerchantOrderModel order)
        {
            return order.OrderStatus == (int)MerchantOrderStatus.WaitingReceipt;
        }
    }
}

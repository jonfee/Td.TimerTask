using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Schedule;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Appoint
{
    /// <summary>
    /// 上门订单超时未支付服务
    /// </summary>
    public sealed class VisitingOrderLatePaymentService : QueueSchedulerService
    {
        /// <summary>
        /// 任务计划数据所在Redis配置
        /// </summary>
        ScheduleRedisConfig config;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public VisitingOrderLatePaymentService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.VisitingOrderLatePayment, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.VisitingOrderLatePayment];
        }

        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<VisitingOrderLateNoPaymentModel>(config.Key) : null;

                    if (null != model)
                    {
                        int duetime = (int)model.LastPaymentTime.Subtract(DateTime.Now).TotalMilliseconds;    //延迟执行时间（以毫秒为单位）

                        if (duetime < 0) duetime = 0;

                        System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, Timeout.Infinite);

                        Schedulers.Add(model.OrderID, timer);
                    }

                    //休眠100毫秒，避免CPU空转
                    Thread.Sleep(100);
                }
            });
        }

        protected override void Execute(object state)
        {
            var model = state as VisitingOrderLateNoPaymentModel;

            if (null == model) return;

            try
            {
                var lastOrder = AppointOrderProvider.GetAppointOrder(model.OrderID);

                if (null == lastOrder) throw new Exception(string.Format("订单(ID:{0})信息已不存在！", model.OrderID));

                bool statusRight = false;

                if (lastOrder.QuoteWays == (int)BusinessServiceQuote.WhenOrder && lastOrder.Status == (int)VisitingServiceOrderStatus.WaitingMerchantReceive && !lastOrder.PaiedTime.HasValue)
                {
                    statusRight = true;
                }
                else if (lastOrder.QuoteWays == (int)BusinessServiceQuote.WhenMeeting && lastOrder.Status == (int)VisitingServiceOrderStatus.UserConfirmQuote && !lastOrder.PaiedTime.HasValue)
                {
                    statusRight = true;
                }

                if (!statusRight) throw new Exception(string.Format("订单(编号:{0})状态已发生变更，不能自动完成收货！", lastOrder.OrderCode));

                //自动取消订单
                bool success = AppointOrderProvider.AutoCancelOrder(lastOrder.OrderID);

                string message = string.Empty;

                if (success)
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未付款，系统已自动取消订单！", lastOrder.OrderCode, lastOrder.BusinessName);
                }
                else
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未付款，系统自动取消订单时操作失败！", lastOrder.OrderCode, lastOrder.BusinessName);
                }

                OutputMessage(message);
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }
            finally
            {
                Schedulers.Remove(model.OrderID);
            }
        }
    }
}

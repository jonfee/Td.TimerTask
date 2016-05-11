using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Schedule;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Appoint
{
    /// <summary>
    /// 预约订单超时未支付服务
    /// </summary>
    public sealed class ReservationOrderLatePaymentService : QueueSchedulerService
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
        public ReservationOrderLatePaymentService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.ReservationOrderLatePayment, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.ReservationOrderLatePayment];
        }

        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<ReservationOrderLateNoPaymentModel>(config.Key) : null;

                    if (null != model)
                    {
                        TimeSpan duetime = model.LastPaymentTime.Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                        if (duetime.Ticks < 0) duetime = TimeoutZero;

                        System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                        //输出消息
                        string message = string.Format("预约服务订单(ID:{0})在{1}天{2}小时{3}分{4}秒后未付款系统将自动取消订单", model.OrderID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);
                        OutputMessage(message);

                        Schedulers.Add(model.OrderID, timer);
                    }

                    //休眠100毫秒，避免CPU空转
                    Thread.Sleep(100);
                }
            });
        }

        protected override void Execute(object state)
        {
            var model = state as ReservationOrderLateNoPaymentModel;

            if (null == model) return;

            try
            {
                var lastOrder = AppointOrderProvider.GetAppointOrder(model.OrderID);

                if (null == lastOrder) throw new CustomException(string.Format("订单(ID:{0})信息已不存在！",model.OrderID));

                bool statusRight = false;

                if (lastOrder.QuoteWays == (int)BusinessServiceQuote.WhenOrder && lastOrder.Status == (int)ReservationServiceOrderStatus.WaitingMerchantReceive && !lastOrder.PaiedTime.HasValue)
                {
                    statusRight = true;
                }
                else if (lastOrder.QuoteWays == (int)BusinessServiceQuote.WhenMeeting && lastOrder.Status == (int)ReservationServiceOrderStatus.UserConfirmSolution && !lastOrder.PaiedTime.HasValue)
                {
                    statusRight = true;
                }

                if (!statusRight) throw new CustomException(string.Format("订单(编号:{0})状态已发生变更，不能自动完成收货！",lastOrder.OrderCode));

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

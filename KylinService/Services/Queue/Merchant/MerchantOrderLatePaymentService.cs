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

namespace KylinService.Services.Queue.Merchant
{
    /// <summary>
    /// 附近购超时未支付服务
    /// </summary>
    public sealed class MerchantOrderLatePaymentService : QueueSchedulerService
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
        public MerchantOrderLatePaymentService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.MerchantOrderLatePayment, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.MerchantOrderLatePayment];
        }

        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<MerchantOrderLateNoPaymentModel>(config.Key) : null;

                    if (null != model)
                    {
                        DateTime lastTime = model.CreateTime.AddMinutes(Startup.MerchantOrderConfig.WaitPaymentMinutes);

                        int duetime = (int)lastTime.Subtract(DateTime.Now).TotalMilliseconds;    //延迟执行时间（以毫秒为单位）

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
            var model = state as MerchantOrderLateNoPaymentModel;

            if (null == model) return;

            try
            {
                var lastOrder = MerchantOrderProvider.GetOrder(model.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (lastOrder.OrderStatus != (int)MerchantOrderStatus.WaitingPayment) throw new Exception("当前订单状态发生变更，不能自动取消订单");

                //自动取消订单
                bool cancelSuccess = MerchantOrderProvider.AutoCancelOrder(lastOrder.OrderID).Result;

                string message = string.Empty;

                if (cancelSuccess)
                {
                    message = string.Format("〖订单（{0}）〗因超时未付款，系统已自动取消订单！", lastOrder.OrderCode);
                }
                else
                {
                    message = string.Format("〖订单（{0}）〗因超时未付款，系统自动取消订单时操作失败！", lastOrder.OrderCode);
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

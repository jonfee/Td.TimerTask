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
    /// 附近购超时未收货服务
    /// </summary>
    public sealed class MerchantOrderLateReceiveService : QueueSchedulerService
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
        public MerchantOrderLateReceiveService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.MerchantOrderLateReceive, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.MerchantOrderLateReceive];
        }

        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<MerchantOrderNoReceiveModel>(config.Key) : null;

                    if (null != model)
                    {
                        DateTime lastTime = model.SendTime.AddDays(Startup.MerchantOrderConfig.WaitReceiptGoodsDays);

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
            var model = state as MerchantOrderNoReceiveModel;

            if (null == model) return;

            try
            {
                var lastOrder = MerchantOrderProvider.GetOrder(model.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (lastOrder.OrderStatus != (int)MerchantOrderStatus.WaitingReceipt) throw new Exception("订单状态已发生变更，不能自动完成收货！");

                if (model.SendTime != lastOrder.SendTime) throw new Exception("未明确的发货时间，不能自动完成收货！");

                //自动收货确认
                bool receiptSuccess = MerchantOrderProvider.AutoReceiptGoods(lastOrder.OrderID).Result;

                string message = string.Empty;

                if (receiptSuccess)
                {
                    message = string.Format("〖订单（{0}）〗因超时未确认收货，系统已自动收货确认处理！", lastOrder.OrderCode);
                }
                else
                {
                    message = string.Format("〖订单（{0}）〗因超时未确认收货，系统自动收货确认处理时操作失败！", lastOrder.OrderCode);
                }

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
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

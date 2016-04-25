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

namespace KylinService.Services.Queue.Mall
{
    /// <summary>
    /// 精品汇超时未收货服务
    /// </summary>
    public sealed class MallOrderLateReceiveService : QueueSchedulerService
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
        public MallOrderLateReceiveService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.MallOrderLateReceive, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.MallOrderLateReceive];
        }

        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<MallOrderLateReceiveModel>(config.Key) : null;

                    if (null != model)
                    {
                        DateTime lastTime = model.ShipTime.AddDays(Startup.B2COrderConfig.WaitReceiptGoodsDays);

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
            var model = state as MallOrderLateReceiveModel;

            if (null == model) return;

            try
            {
                var lastOrder = MallOrderProvider.GetOrder(model.OrderID);

                if (null == lastOrder) throw new Exception("订单信息已不存在！");

                if (lastOrder.OrderStatus != (int)B2COrderStatus.WaitingReceipt) throw new Exception("订单状态已发生变更，不能自动完成收货！");

                if (model.ShipTime != model.ShipTime) throw new Exception("不能确定发货时间，不能自动完成收货！");

                //自动收货确认
                bool receiptSuccess = MallOrderProvider.AutoReceiptGoods(lastOrder.OrderID).Result;

                string message = string.Empty;

                if (receiptSuccess)
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未确认收货，系统已自动收货确认处理！", lastOrder.OrderCode, lastOrder.ProductInfo);
                }
                else
                {
                    message = string.Format("〖订单（{0}）：{1}〗因超时未确认收货，系统自动收货确认处理时操作失败！", lastOrder.OrderCode, lastOrder.ProductInfo);
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

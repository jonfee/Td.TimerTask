using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Data.Settlement;
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
    /// 上门超时间未确认服务结束的服务
    /// </summary>
    public sealed class VisitingOrderLateConfirmDoneService : QueueSchedulerService
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
        public VisitingOrderLateConfirmDoneService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.VisitingOrderLateConfirmDone, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.VisitingOrderLateConfirmDone];
        }

        public override void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                while (true)
                {
                    //获取一条待处理数据
                    var model = null != config ? config.DataBase.ListLeftPop<VisitingOrderLateReceiveModel>(config.Key) : null;

                    if (null != model)
                    {
                        TimeSpan duetime = model.WorkerFinishTime.AddDays(Startup.AppointConfig.EndServiceWaitUserDays).Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                        if (duetime.Ticks < 0) duetime = TimeoutZero;

                        System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                        //输出消息
                        string message = string.Format("上门服务订单(ID:{0})在{1}天{2}小时{3}分{4}秒后未确认服务完成系统将自动确认服务完成", model.OrderID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);
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
            var model = state as VisitingOrderLateReceiveModel;

            if (null == model) return;

            try
            {
                var lastOrder = AppointOrderProvider.GetAppointOrder(model.OrderID);

                if (null == lastOrder) throw new Exception(string.Format("订单(ID:{0})信息已不存在！", model.OrderID));

                if (lastOrder.Status != (int)VisitingServiceOrderStatus.WorkerServiceDone) throw new Exception(string.Format("订单(编号:{0})状态已发生变更，不能自动确认服务完成！", lastOrder.OrderCode));

                //结算并自动收货
                var settlement = new VisitingOrderSettlementCenter(model.OrderID, true);
                settlement.Execute();

                string message = string.Empty;

                if (settlement.Success)
                {
                    message = string.Format("〖上门订单（{0}）〗自动确认服务完成！", lastOrder.OrderCode);
                }
                else
                {
                    message = string.Format("〖上门订单（{0}）〗自动确认服务完成失败，原因：{1}", lastOrder.OrderCode, settlement.ErrorMessage);
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

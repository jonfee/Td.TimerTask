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
        public MerchantOrderLateReceiveService() : base(QueueScheduleType.MerchantOrderLateReceive)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.MerchantOrderLateReceive];
        }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            //获取一条待处理数据
            var model = null != config ? config.DataBase.ListLeftPop<MerchantOrderNoReceiveModel>(config.Key) : null;

            if (null != model)
            {
                    DateTime lastTime = model.SendTime.AddDays(Startup.MerchantOrderConfig.WaitReceiptGoodsDays);

                    TimeSpan duetime = lastTime.Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                    if (duetime.Ticks < 0) duetime = TimeoutZero;

                    System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                    //输出消息
                    string message = string.Format("附近购订单(ID:{0})在{1}天{2}小时{3}分{4}秒后未收货系统将自动确认收货", model.OrderID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);

                Logger(message);

                    Schedulers.Add(model.OrderID, timer);

                return true;
            }

            return false;
        }

        protected override void Execute(object state)
        {
            var model = state as MerchantOrderNoReceiveModel;

            if (null == model) return;

            try
            {
                var lastOrder = MerchantOrderProvider.GetOrder(model.OrderID);

                if (null == lastOrder) throw new CustomException(string.Format("订单(ID:{0})信息已不存在！",model.OrderID));

                if (lastOrder.OrderStatus != (int)MerchantOrderStatus.WaitingReceipt) throw new CustomException(string.Format("订单(编号{0})状态已发生变更，不能自动完成收货！",lastOrder.OrderCode));

                if (model.SendTime != lastOrder.SendTime) throw new CustomException(string.Format("订单(编号{0})未明确的发货时间，不能自动完成收货！",lastOrder.OrderCode));

                //结算并自动收货
                var settlement = new MerchantOrderSettlementCenter(model.OrderID, true);
                settlement.Execute();

                string message = string.Empty;

                if (settlement.Success)
                {
                    message = string.Format("〖订单（{0}）〗自动确认收货完成！", lastOrder.OrderCode);
                }
                else
                {
                    message = string.Format("〖订单（{0}）〗自动确认收货失败，原因：{1}", lastOrder.OrderCode, settlement.ErrorMessage);
                }

                Logger(message);
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

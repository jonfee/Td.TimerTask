using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Threading;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Mall
{
    /// <summary>
    /// 精品汇超时未支付服务
    /// </summary>
    public sealed class MallOrderLatePaymentService : QueueSchedulerService<MallOrderNoPaymentModel>
    {
        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public MallOrderLatePaymentService() : base(QueueScheduleType.MallOrderLatePayment) { }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            if (null == RedisConfig) return false;

            if (null == QuequDatabase)
            {
                WriteMessageHelper.WriteMessage("Redis(database)连接丢失，source:" + this.ServiceName + "，Method:" + this.Me());
                return false;
            }

            //获取一条待处理数据
            var model = QuequDatabase.ListLeftPop<MallOrderNoPaymentModel>(RedisConfig.Key);

            return EntityTaskHandler(model);
        }

        protected override void Execute(object state)
        {
            var model = state as MallOrderNoPaymentModel;

            if (null == model) return;

            try
            {
                //从备份区将备份删除
                DeleteBackAfterDone(model.OrderID);

                var lastOrder = MallOrderProvider.GetOrder(model.OrderID);

                if (null == lastOrder) throw new CustomException(string.Format("〖精品汇订单（ID:{0}）〗信息已不存在！", model.OrderID));

                if (lastOrder.OrderStatus != (int)B2COrderStatus.WaitingPayment) throw new CustomException(string.Format("〖精品汇订单（ID:{0}）〗状态发生变更，不能自动取消订单",lastOrder.OrderID));

                //自动取消订单
                bool cancelSuccess = MallOrderProvider.AutoCancelOrder(lastOrder.OrderID).Result;

                string message = string.Empty;

                if (cancelSuccess)
                {
                    message = string.Format("〖精品汇订单（ID:{0}/{1}）〗因超时未付款，系统已自动取消订单！", lastOrder.OrderID, lastOrder.ProductInfo);
                }
                else
                {
                    message = string.Format("〖精品汇订单（ID:{0}/{1}）〗因超时未付款，系统自动取消订单时操作失败！", lastOrder.OrderID, lastOrder.ProductInfo);
                }

                RunLogger(message);
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

        protected override bool EntityTaskHandler(MallOrderNoPaymentModel model, bool mustBackup = true)
        {
            //对象不为null
            if (null != model)
            {
                DateTime lastTime = model.NeedPayTime ?? model.CreateTime.AddMinutes(Startup.B2COrderConfig.WaitPaymentMinutes);

                TimeSpan duetime = lastTime.Subtract(DateTime.Now);    //延迟执行时间

                if (duetime.Ticks < 0) duetime = TimeoutZero;

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                if (mustBackup)
                {
                    //复制到备份区以防数据丢失
                    BackBeforeDone(model.OrderID, model);
                }

                //输出消息 
                string message = string.Format("〖精品汇订单（ID:{0}）〗在{1}天{2}小时{3}分{4}秒后未付款系统将自动取消订单", model.OrderID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);

                RunLogger(message);

                Schedulers.Add(model.OrderID, timer);

                return true;
            }

            return false;
        }
    }
}

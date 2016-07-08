using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KylinService.Core;
using KylinService.Data.Model;
using KylinService.Data.Provider;
using KylinService.Redis.Schedule;
using KylinService.Services.Queue.Legwork.Model;
using KylinService.SysEnums;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Legwork
{
    public class Legwork_OrderTimeoutService : QueueSchedulerService
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
        public Legwork_OrderTimeoutService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(QueueScheduleType.LegworkOrderTimeout, form, writeDelegate)
        {
            config = Startup.ScheduleRedisConfigs[QueueScheduleType.LegworkOrderTimeout];
        }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            //获取一条待处理数据
            var model = null != config ? config.DataBase.ListLeftPop<OrderTimeoutModel>(config.Key) : null;

            if (null != model)
            {
                //非脏数据则处理
                if (model.OrderID.ToString() != Startup.DirtyDataPKValue)
                {
                    DateTime lastTime = model.CreateTime.AddSeconds(Startup.LegworkGlobalConfig.OrderTimeout);

                    TimeSpan duetime = lastTime.Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                    if (duetime.Ticks < 0) duetime = TimeoutZero;

                    System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                    //输出消息
                    string message = string.Format("跑腿订单(ID:{0})在{1}天{2}小时{3}分{4}秒后没有员工接单，系统将自动取消订单", model.OrderID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);
                    OutputMessage(message);

                    Schedulers.Add(model.OrderID, timer);
                }

                return true;
            }

            return false;
        }

        protected override void Execute(object state)
        {
            var model = state as OrderTimeoutModel;

            if (null == model) return;

            try
            {
                var lastOrder = LegworkOrderProvder.GetLegworkOrder(model.OrderID);

                if (null == lastOrder) throw new CustomException(string.Format("订单(ID:{0})信息已不存在！", model.OrderID));

                if (lastOrder.Status != (int)LegworkOrderStatus.WaitingHandle) throw new CustomException(string.Format("订单(编号{0})状态已发生变更，不能取消订单！", lastOrder.OrderCode));


                //结算并自动收货
                var settlement = LegworkOrderProvder.UpdateOrderTimeout(lastOrder);

                string message = string.Empty;

                if (settlement.Result)
                {
                    message = string.Format("〖订单（{0}）〗自动取消订单完成！", lastOrder.OrderCode);
                }
                else
                {
                    message = string.Format("〖订单（{0}）〗自动取消订单失败！", lastOrder.OrderCode);
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

        protected override void WriteDirtyData()
        {
            var model = new OrderTimeoutModel();
            model.OrderID = long.Parse(Startup.DirtyDataPKValue);

            config.DataBase.ListRightPush(config.Key, model);
        }
    }
}

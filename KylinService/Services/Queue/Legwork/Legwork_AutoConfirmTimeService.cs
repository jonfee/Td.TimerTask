﻿using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.Redis.Schedule.Model;
using KylinService.SysEnums;
using System;
using System.Threading;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue.Legwork
{
    public class Legwork_AutoConfirmTimeService : QueueSchedulerService<LegworkAutoConfirmTimeModel>
    {
        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public Legwork_AutoConfirmTimeService() : base(QueueScheduleType.LegworkAutoConfirmTime) { }

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
            var model = QuequDatabase.ListLeftPop<LegworkAutoConfirmTimeModel>(RedisConfig.Key);

            return EntityTaskHandler(model);
        }

        protected override void Execute(object state)
        {
            var model = state as LegworkAutoConfirmTimeModel;

            if (null == model) return;

            try
            {
                //从备份区将备份删除
                DeleteBackAfterDone(model.OrderID);

                var lastOrder = LegworkOrderProvder.GetLegworkOrder(model.OrderID);

                if (null == lastOrder) throw new CustomException(string.Format("〖跑腿订单（ID:{0}）〗信息已不存在！", model.OrderID));

                if (lastOrder.Status != (int)LegworkOrderStatus.Delivered) throw new CustomException(string.Format("〖跑腿订单（ID:{0}）〗状态已发生变更，不能自动完成收货！", lastOrder.OrderID));

                //if (model.ShipTime != model.ShipTime) throw new CustomException(string.Format("订单(编号{0})不能确定发货时间，不能自动完成收货！", lastOrder.OrderCode));

                //结算并自动收货
                var settlement = LegworkOrderProvder.UpdateAutoConfirmTime(lastOrder);

                string message = string.Empty;

                if (settlement.Result)
                {
                    message = string.Format("〖跑腿订单（ID:{0}）〗自动确认收货完成！", lastOrder.OrderID);
                }
                else
                {
                    message = string.Format("〖跑腿订单（ID:{0}）〗自动确认收货失败！", lastOrder.OrderID);
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

        protected override bool EntityTaskHandler(LegworkAutoConfirmTimeModel model, bool mustBackup = true)
        {
            if (null != model)
            {
                DateTime lastTime = model.ActualDeliveryTime.Value.AddSeconds(Startup.LegworkGlobalConfig.AutoConfirmTime);

                TimeSpan duetime = lastTime.Subtract(DateTime.Now);    //延迟执行时间（以毫秒为单位）

                if (duetime.Ticks < 0) duetime = TimeoutZero;

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(Execute), model, duetime, TimeoutInfinite);

                if (mustBackup)
                {
                    //复制到备份区以防数据丢失
                    BackBeforeDone(model.OrderID, model);
                }

                //输出消息
                string message = string.Format("〖跑腿订单（ID:{0}）〗在{1}天{2}小时{3}分{4}秒后未收货系统将自动确认收货", model.OrderID, duetime.Days, duetime.Hours, duetime.Minutes, duetime.Seconds);

                RunLogger(message);

                Schedulers.Add(model.OrderID, timer);

                return true;
            }

            return false;
        }
    }
}

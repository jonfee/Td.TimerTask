﻿using KylinService.Core;
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

                if (null == lastOrder) throw new Exception(string.Format("订单(ID:{0})信息已不存在！",model.OrderID));

                if (lastOrder.OrderStatus != (int)MerchantOrderStatus.WaitingReceipt) throw new Exception(string.Format("订单(编号{0})状态已发生变更，不能自动完成收货！",lastOrder.OrderCode));

                if (model.SendTime != lastOrder.SendTime) throw new Exception(string.Format("订单(编号{0})未明确的发货时间，不能自动完成收货！",lastOrder.OrderCode));

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

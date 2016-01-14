using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.SysEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 商城订单处理服务
    /// </summary>
    public class MallOrderService : BaseService
    {
        /// <summary>
        /// 配置
        /// </summary>
        private OrderLateConfig Config;

        /// <summary>
        /// 订单
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public MallOrderService(OrderLateConfig config, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.ServiceName = SysData.GetServiceName((int)ScheduleType.MallOrderLate);
            this.Config = config;
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="parameters"></param>
        protected override void OnStart(params object[] parameters)
        {
            string beforeMessage = string.Format("{0} 商城订单数据统计中……", ServiceName);
            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, beforeMessage);

            //获取今天将超时的订单
            var list = MallOrderProvider.GetListForTodayWillTimeout(Config);

            //写入订单处理计划任务
            if (null != list && list.Count > 0)
            {
                MallOrderLateSchedulerManager.CheckScheduler(list.Select(p => p.OrderID).ToArray());

                foreach (var order in list)
                {
                    if (null != order)
                    {
                        MallOrderLateSchedulerManager.StartScheduler(this.Config, order, this.CurrentForm, this.WriteDelegate);
                    }
                }
            }

            //获取福利开奖的任务计划集合
            ICollection scheduleList = MallOrderLateSchedulerManager.Schedulers.Values;

            //输出待开奖数量及福利情况
            string message = string.Format("商城订单数据统计完成，今日共有 {0} 个订单等待处理！分别是：", scheduleList.Count);
            DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);

            foreach (var val in scheduleList)
            {
                var schedule = val as MallOrderLateScheduler;

                var order = schedule.Order;

                var timeout = MallOrderTimeCalculator.GetTimeoutTime(order, Config);

                var dueTime = timeout - DateTime.Now;

                string welPut = string.Format("【订单（{0}）：{1}】将在{2}小时{3}分{4}秒后开奖", order.OrderCode, order.ProductInfo, dueTime.Hours, dueTime.Minutes, dueTime.Seconds);

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, welPut);
            }
        }
    }
}

using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.SysEnums;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace KylinService.Services.Appoint
{
    public class AppointService : BaseService
    {
        /// <summary>
        /// 配置
        /// </summary>
        private AppointConfig Config;

        public AppointService(AppointConfig config, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.ServiceType = ScheduleType.AppointOrderLate.ToString();

            this.Config = config;
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="parameters"></param>
        protected override void OnStart(params object[] parameters)
        {
            string beforeMessage = string.Format("{0} 上门/预约订单数据统计中……", ServiceName);
            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, beforeMessage);

            //获取今天将超时支付的订单
            var lateNoPayList = AppointOrderProvider.GetPaymentLateListForTodayWillTimeout(Config.PaymentWaitMinutes);

            //获取今天将超时确认服务完成的订单
            var lateUserFinishList = AppointOrderProvider.GetUserFinishLateListForTodayWillTimeout(Config.EndServiceWaitUserDays);

            var orderIDs = lateNoPayList.Select(p => p.OrderID).Concat(lateUserFinishList.Select(p => p.OrderID)).ToArray();

            //校正任务列表
            AppointOrderLateSchedulerManager.CheckScheduler(orderIDs);

            //将超时支付的订单写入计划任务
            if (null != lateNoPayList && lateNoPayList.Count > 0)
            {   
                foreach (var order in lateNoPayList)
                {
                    if (null != order)
                    {
                        AppointOrderLateSchedulerManager.StartScheduler(AppointLateType.LateNoPayment, this.Config, order, this.CurrentForm, this.WriteDelegate);
                    }
                }
            }

            //将超时未确认服务完成的订单写入计划任务
            if (null != lateUserFinishList && lateUserFinishList.Count > 0)
            {
                foreach (var order in lateUserFinishList)
                {
                    if (null != order)
                    {
                        AppointOrderLateSchedulerManager.StartScheduler(AppointLateType.LateUserFinish, this.Config, order, this.CurrentForm, this.WriteDelegate);
                    }
                }
            }

            //获取上門預約的任务计划集合
            ICollection scheduleList = AppointOrderLateSchedulerManager.Schedulers.Values;

            //输出待自动处理的数量及订单情况
            string message = string.Format("上门预约订单数据统计完成，今日共有 {0} 个订单等待处理！分别是：", scheduleList.Count);
            DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);

            foreach (var val in scheduleList)
            {
                var schedule = val as BaseAppointOrderLateScheduler;

                var order = schedule.Order;

                var timeout = DateTime.Now.Date;

                string tips = string.Empty;

                if (schedule is AppointOrderPaymentLateScheduler)
                {
                    timeout = AppointOrderTimeCalculator.GetTimeoutTime(order, Config, AppointLateType.LateNoPayment);
                    tips = "自动取消";
                }
                else if (schedule is AppointOrderUserFinishLateScheduler)
                {
                    timeout = AppointOrderTimeCalculator.GetTimeoutTime(order, Config, AppointLateType.LateUserFinish);
                    tips = "自动确认服务完成";
                }

                var dueTime = timeout - DateTime.Now;

                string welPut = string.Format("【订单（{0}）：{1}】将在{2}小时{3}分{4}秒后{5}", order.OrderCode, order.BusinessName, dueTime.Hours, dueTime.Minutes, dueTime.Seconds, tips);

                DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, welPut);
            }
        }
    }
}

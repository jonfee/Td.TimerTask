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
            this.ServiceType = ScheduleType.MallOrderLate.ToString();

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

            //获取今天超时未支付的订单
            var nopayList = MallOrderProvider.GetNoPaymentListForTodayWillTimeout(Config.WaitPaymentHours);

            //获取今天超时未确认收货的订单
            var noconfirmReceiptGoodsList = MallOrderProvider.GetNoConfirmReceiptGoodsListForTodayWillTimeout(Config.WaitReceiptGoodsDays);

            var orderIDs = nopayList.Union(noconfirmReceiptGoodsList).Select(p => p.OrderID).ToArray();

            //校正计划列表
            MallOrderLateSchedulerManager.CheckScheduler(orderIDs);

            //超时未支付的订单写入计划任务
            if (null != nopayList && nopayList.Count > 0)
            {
                foreach (var order in nopayList)
                {
                    if (null != order)
                    {
                        MallOrderLateSchedulerManager.StartScheduler(SysEnums.MallOrderLateType.LateNoPayment, this.Config, order, this.CurrentForm, this.WriteDelegate);
                    }
                }
            }

            //超时未确认收货的订单写入计划任务
            if (null != noconfirmReceiptGoodsList && noconfirmReceiptGoodsList.Count > 0)
            {
                foreach (var order in noconfirmReceiptGoodsList)
                {
                    if (null != order)
                    {
                        MallOrderLateSchedulerManager.StartScheduler(SysEnums.MallOrderLateType.LateUserFinish, this.Config, order, this.CurrentForm, this.WriteDelegate);
                    }
                }
            }

            //获取福利开奖的任务计划集合
            ICollection scheduleList = MallOrderLateSchedulerManager.Schedulers.Values;

            StringBuilder sbMessage = new StringBuilder();

            //输出待开奖数量及福利情况
            string message = string.Format("商城订单数据统计完成，今日共有 {0} 个订单等待处理！分别是：", scheduleList.Count);
            sbMessage.AppendLine(message);

            foreach (var val in scheduleList)
            {
                var schedule = val as BaseMallOrderLateScheduler;

                var order = schedule.Order;

                var timeout = DateTime.Now.Date;

                var tips = string.Empty;

                if (schedule is MallOrderPaymentLateScheduler)
                {
                    timeout = MallOrderTimeCalculator.GetTimeoutTime(order, Config, MallOrderLateType.LateNoPayment);
                    tips = "自动取消";
                }
                else if (schedule is MallOrderLateUserFinishScheduler)
                {
                    timeout = MallOrderTimeCalculator.GetTimeoutTime(order, Config, MallOrderLateType.LateUserFinish);
                    tips = "自动确认收货";
                }

                var dueTime = timeout - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                string welPut = string.Format("【订单（{0}）：{1}】将在{2}小时{3}分{4}秒后[{5}]{6}", order.OrderCode, order.ProductInfo, dueTime.Hours, dueTime.Minutes, dueTime.Seconds, timeout.ToString("yyyy/MM/dd HH:mm:ss"), tips);

                sbMessage.AppendLine(welPut);
            }

            DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, sbMessage.ToString());
        }
    }
}

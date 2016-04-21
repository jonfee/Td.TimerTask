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

namespace KylinService.Services.MerchantOrderLate
{
    /// <summary>
    /// 商家订单处理服务
    /// </summary>
    public class MerchantOrderService : BaseService
    {
        /// <summary>
        /// 配置
        /// </summary>
        private MerchantOrderLateConfig Config;

        /// <summary>
        /// 订单
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public MerchantOrderService(MerchantOrderLateConfig config, Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        { 
            this.ServiceType = ScheduleType.MerchantOrderLate.ToString();

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
            var nopayList = MerchantOrderProvider.GetNoPaymentListForTodayWillTimeout(Config.WaitPaymentMinutes);

            //获取今天超时未确认收货的订单
            var noconfirmReceiptGoodsList = MerchantOrderProvider.GetNoConfirmReceiptGoodsListForTodayWillTimeout(Config.WaitReceiptGoodsDays);

            var orderIDs = nopayList.Union(noconfirmReceiptGoodsList).Select(p => p.OrderID).ToArray();

            if (null != orderIDs && orderIDs.Length > 0)
            {
                //校正计划列表
                MerchantOrderLateSchedulerManager.Instance.CheckScheduler(orderIDs);

                //超时未支付的订单写入计划任务
                if (null != nopayList && nopayList.Count > 0)
                {
                    foreach (var order in nopayList)
                    {
                        if (null != order)
                        {
                            MerchantOrderLateSchedulerManager.Instance.StartScheduler(SysEnums.MerchantOrderLateType.LateNoPayment, this.Config, order, this.CurrentForm, this.WriteDelegate);
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
                            MerchantOrderLateSchedulerManager.Instance.StartScheduler(SysEnums.MerchantOrderLateType.LateUserFinish, this.Config, order, this.CurrentForm, this.WriteDelegate);
                        }
                    }
                }
            }
            else
            {
                MerchantOrderLateSchedulerManager.Instance.Clear();
            }

            //获取福利开奖的任务计划集合
            ICollection scheduleList = MerchantOrderLateSchedulerManager.Schedulers.Values;

            StringBuilder sbMessage = new StringBuilder();

            //输出待开奖数量及福利情况
            string message = string.Format("商城订单数据统计完成，今日共有 {0} 个订单等待处理！{1}", scheduleList.Count, scheduleList.Count > 0 ? "分别是：" : "");
            sbMessage.AppendLine(message);

            foreach (var val in scheduleList)
            {
                var schedule = val as BaseMerchantOrderLateScheduler;

                var order = schedule.Order;

                var timeout = DateTime.Now.Date;

                var tips = string.Empty;

                if (schedule is MerchantOrderPaymentLateScheduler)
                {
                    timeout = MerchantOrderTimeCalculator.GetTimeoutTime(order, Config, MerchantOrderLateType.LateNoPayment);
                    tips = "自动取消";
                }
                else if (schedule is MerchantOrderLateUserFinishScheduler)
                {
                    timeout = MerchantOrderTimeCalculator.GetTimeoutTime(order, Config, MerchantOrderLateType.LateUserFinish);
                    tips = "自动确认收货";
                }

                var dueTime = timeout - DateTime.Now;

                dueTime = dueTime.CheckPositive();

                string welPut = string.Format("【订单（{0}】将在{1}小时{2}分{3}秒后[{4}]{5}", order.OrderCode,  dueTime.Hours, dueTime.Minutes, dueTime.Seconds, timeout.ToString("yyyy/MM/dd HH:mm:ss"), tips);

                sbMessage.AppendLine(welPut);
            }

            DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, sbMessage.ToString());
        }
    }
}

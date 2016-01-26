using System;

namespace KylinService.Redis.Models
{
    /// <summary>
    /// 福利中奖消息
    /// </summary>
    public class WelfareWinnerContent
    {
        /// <summary>
        /// 福利ID
        /// </summary>
        public long WelfareID { get; set; }

        /// <summary>
        /// 福利类型
        /// </summary>
        public int WelfareType { get; set; }

        /// <summary>
        /// 福利活动ID
        /// </summary>
        public long WelfarePhaseID { get; set; }

        /// <summary>
        /// 福利名称
        /// </summary>
        public string WelfareName { get; set; }

        /// <summary>
        /// 福利提供商ID
        /// </summary>
        public long MerchantID { get; set; }

        /// <summary>
        /// 福利提供商名称
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>
        /// 中奖用户ID
        /// </summary>
        public long[] UserIDs { get; set; }

        /// <summary>
        /// 中奖时间
        /// </summary>
        public DateTime LolleryTime { get; set; }
    }
}

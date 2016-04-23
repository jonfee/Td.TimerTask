using System;

namespace KylinService.Redis.Push.Model
{
    /// <summary>
    /// 福利报名提醒消息内容
    /// </summary>
    public class WelfareRemindContent
    {
        /// <summary>
        /// 福利ID
        /// </summary>
        public long WelfareID { get; set; }

        /// <summary>
        /// 福利名称
        /// </summary>
        public string Name { get; set; }

        ///<summary>
        ///商户ID
        ///</summary>
        public long MerchantID { get; set; }

        ///<summary>
        ///商户名称
        ///</summary>
        public string MerchantName { get; set; }

        /// <summary>
        /// 参与用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 接受报名的开始时间
        /// </summary>
        public DateTime ApplyStartTime { get; set; }

        /// <summary>
        /// 商家预发放数量
        /// </summary>
        public int Number { get; set; }
    }
}

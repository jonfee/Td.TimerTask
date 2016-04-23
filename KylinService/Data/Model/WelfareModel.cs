using System;

namespace KylinService.Data.Model
{
    /// <summary>
    /// 福利模型
    /// </summary>
    public class WelfareModel
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
       /// 福利名称
       /// </summary>
        public string WelfareName { get; set; }

        /// <summary>
        /// 福利提供商
        /// </summary>
        public string MarchantName { get; set; }

        /// <summary>
        /// 发放数量
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 接受报名的开始时间
        /// </summary>
        public DateTime? ApplyStartTime { get; set; }

        /// <summary>
        /// 参与人数
        /// </summary>
        public int PartNumber { get; set; }

        /// <summary>
        /// 中奖人数
        /// </summary>
        public int WinNumber { get; set; }

        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime LotteryTime { get; set; }

        /// <summary>
        /// 福利状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否已被删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 有效期（起）
        /// </summary>
        public DateTime ExpiryStartTime { get; set; }

        /// <summary>
        /// 有效期（止）
        /// </summary>
        public DateTime ExpiryEndTime { get; set; }
    }
}

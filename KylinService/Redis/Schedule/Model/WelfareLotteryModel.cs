using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 福利开奖数据模型
    /// </summary>
    public class WelfareLotteryModel
    {
        /// <summary>
        /// 福利ID
        /// </summary>
        public long WelfareID { get; set; }
        
        /// <summary>
        /// 福利名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime LotteryTime { get; set; }
    }
}

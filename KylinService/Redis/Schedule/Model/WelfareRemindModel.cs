using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 福利参与报名提醒
    /// </summary>
    public class WelfareRemindModel
    {
        /// <summary>
        /// 福利ID
        /// </summary>
        public long WelfareID { get; set; }

        /// <summary>
        /// 接受报名的开始时间
        /// </summary>
        public DateTime ApplyStartTime { get; set; }
    }
}

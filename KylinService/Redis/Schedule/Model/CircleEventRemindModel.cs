using KylinService.Services;
using System;

namespace KylinService.Redis.Schedule.Model
{
    /// <summary>
    /// 社区活动开始提醒
    /// </summary>
    public class CircleEventRemindModel : ServiceState
    {
        ///<summary>
		///活动ID
		///</summary>
		public long EventID { get; set; }

        ///<summary>
        ///开始时间
        ///</summary>
        public DateTime StartTime { get; set; }
    }
}

using System;

namespace KylinService.Data.Model
{
    /// <summary>
    /// 社区活动模型
    /// </summary>
    public class CircleEventModel
    {
        ///<summary>
		///活动ID
		///</summary>
		public long EventID { get; set; }
        
        ///<summary>
        ///开始时间
        ///</summary>
        public DateTime StartTime { get; set; }

        ///<summary>
        ///结束时间
        ///</summary>
        public DateTime EndTime { get; set; }
        
        ///<summary>
        ///活动地点
        ///</summary>
        public string Address { get; set; }
        
        ///<summary>
        ///活动状态
        ///</summary>
        public int EventStatus { get; set; }
    }
}

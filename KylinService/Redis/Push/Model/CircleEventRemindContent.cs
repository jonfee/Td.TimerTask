using System;

namespace KylinService.Redis.Push.Model
{
    /// <summary>
    /// 社区活动开始提醒消息内容
    /// </summary>
    public class CircleEventRemindContent
    {
        ///<summary>
		///活动ID
		///</summary>
		public long EventID { get; set; }

        ///<summary>
		///主题帖子ID
		///</summary>
		public long TopicID { get; set; }

        ///<summary>
        ///用户ID
        ///</summary>
        public long UserID { get; set; }

        ///<summary>
        ///用户昵称
        ///</summary>
        public string Username { get; set; }

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
        ///参加活动的用户数
        ///</summary>
        public int UserCount { get; set; }
    }
}

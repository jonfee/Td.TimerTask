using KylinService.Data.Model;
using KylinService.Redis.Push.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KylinService.Data.Provider
{
    public class CircleProvider
    {
        /// <summary>
        /// 获取活动信息
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public static CircleEventModel GetEvent(long eventID)
        {
            using (var db = new DataContext())
            {
                var query = from p in db.Circle_Event
                            where p.EventID == eventID
                            select new CircleEventModel
                            {
                                Address = p.Address,
                                EventID = p.EventID,
                                EndTime = p.EndTime,
                                EventStatus = p.EventStatus,
                                StartTime = p.StartTime
                            };

                return query.FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取需要活动提醒的消息内容集合
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public static List<CircleEventRemindContent> GetRemindContentList(long eventID)
        {
            using (var db = new DataContext())
            {
                var query = from p in db.Circle_EventUser
                            join e in db.Circle_Event
                            on p.EventID equals e.EventID
                            where e.EventID == eventID && p.NeedRemind == true
                            select new CircleEventRemindContent
                            {
                                EventID = p.EventID,
                                UserID = p.UserID,
                                Username = p.Username,
                                Address = e.Address,
                                EndTime = e.EndTime,
                                StartTime = e.StartTime,
                                UserCount = e.UserCount,
                                TopicID = e.TopicID
                            };

                return query.ToList();
            }
        }
    }
}

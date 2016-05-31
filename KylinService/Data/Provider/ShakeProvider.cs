using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Td.Kylin.Entity;

namespace KylinService.Data.Provider
{
    public class ShakeProvider
    {
        /// <summary>
        /// 重置摇一摇每日已摇次数
        /// </summary>
        /// <param name="initCount"></param>
        /// <returns></returns>
        public static int ResetDayTimes()
        {
            using (var db = new DataContext())
            {
                var userIds = db.User_ShakeRecord.Select(p => p.UserID);

                if (null != userIds && userIds.Count() > 0)
                {
                    List<User_ShakeRecord> list = new List<User_ShakeRecord>();

                    foreach (var id in userIds)
                    {
                        list.Add(new User_ShakeRecord { UserID = id, TodayCount = 0 });
                    }
                    
                    db.User_ShakeRecord.AttachRange(list);

                    return db.SaveChanges();
                }

                return 0;
            }
        }
    }
}

using System.Linq;

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
                //return db.User_ShakeRecord.Where(p => p.TodayCount > 0).Update(p => new User_ShakeRecord { TodayCount = 0 });

                var users = db.User_ShakeRecord.ToList();//.Where(p => p.TodayCount > 0)

                users.ForEach((item) =>
                {
                    db.User_ShakeRecord.Attach(item);
                    db.Entry(item).Property(p => p.TodayCount).IsModified = true;
                    item.TodayCount = 0;
                });

                return db.SaveChanges();
            }
        }
    }
}

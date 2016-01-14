using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var list = db.User_ShakeRecord.ToList();

                list.ForEach((item) =>
                {
                    item.TodayCount = 0;
                });

                return db.SaveChanges();
            }
        }
    }
}

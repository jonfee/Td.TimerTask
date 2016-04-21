using System;
using System.Linq;
using Td.Kylin.Entity;

namespace KylinService.Data.Provider
{
    public class UserProvider
    {
        /// <summary>
        /// /根据手机号获取用户信息
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static User_Account GetUserIDByMobile(string mobile)
        {
            using (var db = new DataContext())
            {
                return db.User_Account.FirstOrDefault(p => p.Mobile == mobile);
            }
        }

        public static void Update(long userID)
        {
            using (var db = new DataContext())
            {
                var user = new User_Account { UserID = userID };

                db.User_Account.Attach(user);

                user.LastTime = DateTime.Now;

                db.SaveChanges();
            }
        }
    }
}

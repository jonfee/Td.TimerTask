using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    internal sealed class UserLevelConfigService<DbContext> : IUserLevelConfigService where DbContext : DataContext, new()
    {
        /// <summary>
        /// 获取所有有效的用户等级配置
        /// </summary>
        /// <returns></returns>
        public List<UserLevelConfigCacheModel> GetEnabledAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.System_Level
                            where p.Enable == true
                            orderby p.Min ascending
                            select new UserLevelConfigCacheModel
                            {
                                Icon = p.Icon,
                                LevelID = p.LevelID,
                                Min = p.Min,
                                Name = p.Name
                            };

                return query.ToList();
            }
        }
    }
}

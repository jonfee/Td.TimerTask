using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    internal sealed class UserEmpiricalConfigService<DbContext> : IUserEmpiricalConfigService where DbContext : DataContext, new()
    {
        public List<UserEmpiricalConfigCacheModel> GetAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.System_EmpiricalConfig
                            select new UserEmpiricalConfigCacheModel
                            {
                                ActivityType = p.ActivityType,
                                MaxLimit = p.MaxLimit,
                                MaxUnit = p.MaxUnit,
                                Repeatable = p.Repeatable,
                                Score = p.Score
                            };

                return query.ToList();
            }
        }
    }
}

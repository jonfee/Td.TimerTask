using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    /// <summary>
    /// 区域数据服务
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    internal sealed class SystemAreaService<DbContext> : ISystemAreaService where DbContext : DataContext, new()
    {
        public List<SystemAreaCacheModel> GetAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.System_Area
                            select new SystemAreaCacheModel
                            {
                                AreaID = p.AreaID,
                                AreaName = p.AreaName,
                                HasChild = p.HasChild,
                                Layer = p.Layer
                            };

                return query.ToList();
            }
        }
    }
}

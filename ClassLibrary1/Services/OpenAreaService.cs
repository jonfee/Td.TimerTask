using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    /// <summary>
    /// 开通区域数据服务
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    internal sealed class OpenAreaService<DbContext> : IOpenAreaService where DbContext : DataContext, new()
    {
        /// <summary>
        /// 获取所有开通区域
        /// </summary>
        /// <returns></returns>
        public List<OpenAreaCacheModel> GetAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.Area_Open
                            select new OpenAreaCacheModel
                            {
                                AreaID = p.AreaID,
                                AreaName = p.AreaName,
                                CreateTime = p.CreateTime,
                                Status = p.Status
                            };

                return query.ToList();
            }
        }
    }
}

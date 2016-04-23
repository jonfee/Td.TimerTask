using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    /// <summary>
    /// 职位类别数据服务
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    internal sealed class JobCategoryService<DbContext> : IJobCategoryService where DbContext : DataContext, new()
    {
        public List<JobCategoryCacheModel> GetAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.Job_Category
                            select new JobCategoryCacheModel
                            {
                                CategoryID = p.CategoryID,
                                Name = p.Name,
                                OrderNo = p.OrderNo,
                                ParentID = p.ParentID,
                                TagStatus = p.TagStatus
                            };

                return query.ToList();
            }
        }
    }
}

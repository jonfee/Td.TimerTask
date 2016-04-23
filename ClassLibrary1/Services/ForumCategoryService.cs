using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    internal sealed class ForumCategoryService<DbContext> : IForumCategoryService where DbContext : DataContext, new()
    {
        public List<ForumCategoryCacheModel> GetEnabledAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.Circle_Category
                            where p.IsDelete == false && p.Disabled == false
                            orderby p.OrderNo descending
                            select new ForumCategoryCacheModel
                            {
                                CategoryID=p.CategoryID,
                                Name=p.Name,
                                OrderNo=p.OrderNo
                            };

                return query.ToList();
            }
        }
    }
}

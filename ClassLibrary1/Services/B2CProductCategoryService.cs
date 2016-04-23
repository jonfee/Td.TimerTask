using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    internal sealed class B2CProductCategoryService<DbContext> : IB2CProductCategoryService where DbContext : DataContext, new()
    {
        public List<B2CProductCategoryCacheModel> GetEnabledAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.Mall_Category
                            where p.IsDelete == false && p.Disabled == false
                            select new B2CProductCategoryCacheModel
                            {
                                AreaID = p.AreaID,
                                CategoryID = p.CategoryID,
                                Ico = p.Ico,
                                Name = p.Name,
                                OrderNo = p.OrderNo
                            };

                return query.ToList();
            }
        }
    }
}

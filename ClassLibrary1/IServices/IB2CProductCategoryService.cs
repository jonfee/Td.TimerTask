using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface IB2CProductCategoryService
    {
        List<B2CProductCategoryCacheModel> GetEnabledAll();
    }
}

using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface IB2CProductCategoryTagService
    {
        /// <summary>
        /// 获取所有B2C商品分类标签
        /// </summary>
        /// <returns></returns>
        List<B2CProductCategoryTagCacheModel> GetAll();
    }
}

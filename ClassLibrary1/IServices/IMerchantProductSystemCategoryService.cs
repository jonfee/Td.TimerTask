using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 商家商品系统分类数据服务接口
    /// </summary>
    internal interface IMerchantProductSystemCategoryService
    {
        /// <summary>
        /// 获取所有有效的分类集合
        /// </summary>
        /// <returns></returns>
        List<MerchantProductSystemCategoryCacheModel> GetEnabledAll();
    }
}

using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 商家行业数据服务接口
    /// </summary>
    internal interface IMerchantIndustryService
    {
        /// <summary>
        /// 获取所有有效的商家行业集合
        /// </summary>
        /// <returns></returns>
        List<MerchantIndustryCacheModel> GetEnabledAll();
    }
}

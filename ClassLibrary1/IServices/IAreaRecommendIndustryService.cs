using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 区域行业推荐数据服务接口
    /// </summary>
    internal interface IAreaRecommendIndustryService
    {
        /// <summary>
        /// 获取所有区域推荐行业集合
        /// </summary>
        /// <returns></returns>
        List<AreaRecommendIndustryCacheModel> GetAll();
    }
}

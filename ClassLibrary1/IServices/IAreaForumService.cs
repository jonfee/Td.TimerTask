using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface IAreaForumService
    {
        /// <summary>
        /// 获取所有有效的区域圈子
        /// </summary>
        /// <returns></returns>
        List<AreaForumCacheModel> GetEnabledAll();
    }
}

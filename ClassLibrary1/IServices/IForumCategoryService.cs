using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface IForumCategoryService
    {
        /// <summary>
        /// 获取所有有效的圈子分类
        /// </summary>
        /// <returns></returns>
        List<ForumCategoryCacheModel> GetEnabledAll();
    }
}

using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface IForumCircleService
    {
        /// <summary>
        /// 获取所有有效的圈子
        /// </summary>
        /// <returns></returns>
        List<ForumCircleCacheModel> GetEnabledAll();
    }
}

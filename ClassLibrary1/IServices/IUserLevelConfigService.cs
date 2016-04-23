using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal  interface IUserLevelConfigService
    {
        /// <summary>
        /// 获取所有有效的用户等级配置
        /// </summary>
        /// <returns></returns>
        List<UserLevelConfigCacheModel> GetEnabledAll();
    }
}

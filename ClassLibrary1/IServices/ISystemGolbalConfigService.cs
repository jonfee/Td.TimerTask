using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface ISystemGolbalConfigService
    {
        List<SystemGolbalConfigCacheModel> GetAll();
    }
}

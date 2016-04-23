using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    internal interface IUserEmpiricalConfigService
    {
        List<UserEmpiricalConfigCacheModel> GetAll();
    }
}

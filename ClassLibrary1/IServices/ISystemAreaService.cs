using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 区域数据服务接口
    /// </summary>
    internal interface ISystemAreaService
    {
        /// <summary>
        /// 获取所有区域
        /// </summary>
        /// <returns></returns>
        List<SystemAreaCacheModel> GetAll();
    }
}

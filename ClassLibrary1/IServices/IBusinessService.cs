using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 上门预约服务业务数据服务接口
    /// </summary>
    internal interface IBusinessService
    {
        /// <summary>
        /// 获取所有有效上门预约服务业务数据集合
        /// </summary>
        /// <returns></returns>
        List<BusinessServiceCacheModel> GetEnabledAll();
    }
}

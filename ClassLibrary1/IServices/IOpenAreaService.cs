using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 开通区域数据服务接口
    /// </summary>
    internal interface IOpenAreaService
    {
        /// <summary>
        /// 获取所有开通区域
        /// </summary>
        /// <returns></returns>
        List<OpenAreaCacheModel> GetAll();
    }
}

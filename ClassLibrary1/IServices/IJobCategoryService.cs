using System.Collections.Generic;
using Td.Kylin.DataCache.CacheModel;

namespace Td.Kylin.DataCache.IServices
{
    /// <summary>
    /// 职位类别数据服务接口
    /// </summary>
    internal interface IJobCategoryService
    {
        List<JobCategoryCacheModel> GetAll();
    }
}

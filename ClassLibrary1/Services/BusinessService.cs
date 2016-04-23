using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    /// <summary>
    /// 上门预约服务业务数据服务
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    internal sealed class BusinessService<DbContext> : IBusinessService where DbContext : DataContext, new()
    {
        /// <summary>
        /// 获取所有有效上门预约服务业务数据集合
        /// </summary>
        /// <returns></returns>
        public List<BusinessServiceCacheModel> GetEnabledAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.KylinService_Business
                            where p.IsDelete == false && p.Disabled == false
                            select new BusinessServiceCacheModel
                            {
                                AllowPerson = p.AllowPerson,
                                BusinessID = p.BusinessID,
                                BusinessType = p.BusinessType,
                                CreateTime = p.CreateTime,
                                Icon = p.Icon,
                                IndustryID = p.IndustryID,
                                IsOpenService = p.IsOpenService,
                                Name = p.Name,
                                OrderNo = p.OrderNo,
                                PayerType = p.PayerType,
                                QuoteWays = p.QuoteWays,
                                TagStatus = p.TagStatus
                            };

                return query.ToList();
            }
        }

    }
}

using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    /// <summary>
    /// 商家行业数据服务
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    internal sealed class MerchantIndustryService<DbContext> : IMerchantIndustryService where DbContext : DataContext, new()
    {
        /// <summary>
        /// 获取所有有效的商家行业集合
        /// </summary>
        /// <returns></returns>
        public List<MerchantIndustryCacheModel> GetEnabledAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.Merchant_Industry
                            where p.Disabled == false
                            select new MerchantIndustryCacheModel
                            {
                                IndustryID = p.IndustryID,
                                Name = p.Name,
                                Layer = p.Layer,
                                OrderNo = p.OrderNo,
                                ParentID = p.ParentID
                            };

                return query.ToList();
            }
        }
    }
}

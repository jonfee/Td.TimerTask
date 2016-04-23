using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 上门预约业务缓存
    /// </summary>
    public sealed class BusinessServiceCache : CacheItem<BusinessServiceCacheModel>
    {
        public BusinessServiceCache() : base(CacheItemType.BusinessServices) { }

        protected override List<BusinessServiceCacheModel> GetCache()
        {
            List<BusinessServiceCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<BusinessServiceCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<BusinessServiceCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.BusinessService.GetEnabledAll();
        }

        protected override void SetCache(List<BusinessServiceCacheModel> data)
        {
            if (null != RedisDB)
            {
                //清除数据缓存
                RedisDB.KeyDelete(CacheKey);

                if (data == null) data = ReadDataFromDB();

                if (null != data && data.Count > 0)
                {

                    var dic = data.ToDictionary(k => (RedisValue)k.HashField, v => v);

                    RedisDB.HashSet(CacheKey, dic);
                }
            }
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Add(BusinessServiceCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(BusinessServiceCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(BusinessServiceCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override BusinessServiceCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<BusinessServiceCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="businessID">业务ID</param>
        /// <returns></returns>
        public BusinessServiceCacheModel Get(long businessID)
        {
            var item = new BusinessServiceCacheModel { BusinessID = businessID };

            return Get(item.HashField);
        }
    }
}

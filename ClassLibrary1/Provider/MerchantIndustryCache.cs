using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 商家行业缓存
    /// </summary>
    public sealed class MerchantIndustryCache : CacheItem<MerchantIndustryCacheModel>
    {
        public MerchantIndustryCache() : base(CacheItemType.MerchantIndustry) { }

        protected override List<MerchantIndustryCacheModel> GetCache()
        {
            List<MerchantIndustryCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<MerchantIndustryCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<MerchantIndustryCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.MerchantIndustryService.GetEnabledAll();
        }

        protected override void SetCache(List<MerchantIndustryCacheModel> data)
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
        public override void Add(MerchantIndustryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(MerchantIndustryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(MerchantIndustryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override MerchantIndustryCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<MerchantIndustryCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="industryID">行业ID</param>
        /// <returns></returns>
        public MerchantIndustryCacheModel Get(long industryID)
        {
            var item = new MerchantIndustryCacheModel { IndustryID = industryID };

            return Get(item.HashField);
        }
    }
}

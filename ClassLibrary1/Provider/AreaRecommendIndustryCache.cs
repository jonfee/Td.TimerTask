using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;
using System;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 区域行业推荐缓存
    /// </summary>
    public sealed class AreaRecommendIndustryCache : CacheItem<AreaRecommendIndustryCacheModel>
    {
        public AreaRecommendIndustryCache() : base(CacheItemType.AreaRecommendIndustry) { }

        protected override List<AreaRecommendIndustryCacheModel> GetCache()
        {
            List<AreaRecommendIndustryCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<AreaRecommendIndustryCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<AreaRecommendIndustryCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.AreaRecommendIndustryService.GetAll();
        }

        protected override void SetCache(List<AreaRecommendIndustryCacheModel> data)
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
        public override void Add(AreaRecommendIndustryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(AreaRecommendIndustryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(AreaRecommendIndustryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override AreaRecommendIndustryCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<AreaRecommendIndustryCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="industryID">行业ID</param>
        /// <returns></returns>
        public AreaRecommendIndustryCacheModel Get(int areaID, long industryID)
        {
            var item = new AreaRecommendIndustryCacheModel { AreaID = areaID, IndustryID = industryID };

            return Get(item.HashField);
        }
    }
}

using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 开通区域数据缓存
    /// </summary>
    public sealed class OpenAreaCache : CacheItem<OpenAreaCacheModel>
    {
        public OpenAreaCache() : base(CacheItemType.OpenArea) { }

        protected override List<OpenAreaCacheModel> GetCache()
        {
            List<OpenAreaCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<OpenAreaCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<OpenAreaCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.OpenAreaService.GetAll();
        }

        protected override void SetCache(List<OpenAreaCacheModel> data)
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
        public override void Add(OpenAreaCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(OpenAreaCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(OpenAreaCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override OpenAreaCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<OpenAreaCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <returns></returns>
        public OpenAreaCacheModel Get(int areaID)
        {
            var item = new OpenAreaCacheModel { AreaID = areaID };

            return Get(item.HashField);
        }
    }
}

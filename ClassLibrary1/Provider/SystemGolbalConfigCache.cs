using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 全局配置缓存
    /// </summary>
    public sealed class SystemGolbalConfigCache : CacheItem<SystemGolbalConfigCacheModel>
    {
        public SystemGolbalConfigCache() : base(CacheItemType.SystemGolbalConfig) { }

        protected override List<SystemGolbalConfigCacheModel> GetCache()
        {
            List<SystemGolbalConfigCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<SystemGolbalConfigCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<SystemGolbalConfigCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.SystemGolbalConfigService.GetAll();
        }

        protected override void SetCache(List<SystemGolbalConfigCacheModel> data)
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
        public override void Add(SystemGolbalConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }
        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(SystemGolbalConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(SystemGolbalConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override SystemGolbalConfigCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<SystemGolbalConfigCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <param name="resourceKey">资源类型子项</param>
        /// <returns></returns>
        public SystemGolbalConfigCacheModel Get(int resourceType,int resourceKey)
        {
            var item = new SystemGolbalConfigCacheModel { ResourceType = resourceType,ResourceKey= resourceKey };

            return Get(item.HashField);
        }
    }
}

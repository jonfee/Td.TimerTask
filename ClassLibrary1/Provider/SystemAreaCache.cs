using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 系统区域缓存
    /// </summary>
    public sealed class SystemAreaCache : CacheItem<SystemAreaCacheModel>
    {
        public SystemAreaCache() : base(CacheItemType.SystemArea) { }

        protected override List<SystemAreaCacheModel> GetCache()
        {
            List<SystemAreaCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<SystemAreaCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<SystemAreaCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.SystemAreaService.GetAll();
        }

        protected override void SetCache(List<SystemAreaCacheModel> data)
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
        public override void Add(SystemAreaCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(SystemAreaCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(SystemAreaCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override SystemAreaCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<SystemAreaCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <returns></returns>
        public SystemAreaCacheModel Get(int areaID)
        {
            var item = new SystemAreaCacheModel { AreaID = areaID };

            return Get(item.HashField);
        }
    }
}

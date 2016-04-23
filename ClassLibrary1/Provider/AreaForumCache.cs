using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 区域圈子缓存
    /// </summary>
    public sealed class AreaForumCache : CacheItem<AreaForumCacheModel>
    {
        public AreaForumCache() : base(CacheItemType.AreaForum) { }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Add(AreaForumCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(AreaForumCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override AreaForumCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<AreaForumCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="areaForumID">区域圈子ID</param>
        /// <returns></returns>
        public AreaForumCacheModel Get(long areaForumID)
        {
            var item = new AreaForumCacheModel { AreaForumID = areaForumID };

            return Get(item.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(AreaForumCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        protected override List<AreaForumCacheModel> GetCache()
        {
            List<AreaForumCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<AreaForumCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<AreaForumCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.AreaForumService.GetEnabledAll();
        }

        protected override void SetCache(List<AreaForumCacheModel> data)
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
    }
}

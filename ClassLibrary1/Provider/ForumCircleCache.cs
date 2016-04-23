using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 系统圈子缓存
    /// </summary>
    public sealed class ForumCircleCache : CacheItem<ForumCircleCacheModel>
    {
        public ForumCircleCache() : base(CacheItemType.ForumCircle) { }

        protected override List<ForumCircleCacheModel> GetCache()
        {
            List<ForumCircleCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<ForumCircleCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<ForumCircleCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.ForumCircleService.GetEnabledAll();
        }

        protected override void SetCache(List<ForumCircleCacheModel> data)
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
        public override void Add(ForumCircleCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(ForumCircleCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(ForumCircleCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override ForumCircleCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<ForumCircleCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="forumID">圈子ID</param>
        /// <returns></returns>
        public ForumCircleCacheModel Get(long forumID)
        {
            var item = new ForumCircleCacheModel { ForumID = forumID };

            return Get(item.HashField);
        }
    }
}

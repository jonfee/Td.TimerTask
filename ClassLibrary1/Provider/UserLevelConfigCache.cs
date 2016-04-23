using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 用户等级规则配置缓存
    /// </summary>
    public sealed class UserLevelConfigCache : CacheItem<UserLevelConfigCacheModel>
    {
        public UserLevelConfigCache() : base(CacheItemType.UserLevelConfig) { }

        protected override List<UserLevelConfigCacheModel> GetCache()
        {
            List<UserLevelConfigCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<UserLevelConfigCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<UserLevelConfigCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.UserLevelConfigService.GetEnabledAll();
        }

        protected override void SetCache(List<UserLevelConfigCacheModel> data)
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
        public override void Add(UserLevelConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(UserLevelConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(UserLevelConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override UserLevelConfigCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<UserLevelConfigCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="levelID">等级ID</param>
        /// <returns></returns>
        public UserLevelConfigCacheModel Get(long levelID)
        {
            var item = new UserLevelConfigCacheModel { LevelID = levelID };

            return Get(item.HashField);
        }
    }
}

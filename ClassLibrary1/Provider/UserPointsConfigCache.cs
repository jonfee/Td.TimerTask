using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 用户积分规则配置缓存
    /// </summary>
    public sealed class UserPointsConfigCache : CacheItem<UserPointsConfigCacheModel>
    {
        public UserPointsConfigCache() : base(CacheItemType.UserPointsConfig) { }

        protected override List<UserPointsConfigCacheModel> GetCache()
        {
            List<UserPointsConfigCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<UserPointsConfigCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<UserPointsConfigCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.UserPointsConfigService.GetAll();
        }

        protected override void SetCache(List<UserPointsConfigCacheModel> data)
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
        public override void Add(UserPointsConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(UserPointsConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(UserPointsConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override UserPointsConfigCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<UserPointsConfigCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="activityType">用户业务活动类型</param>
        /// <returns></returns>
        public UserPointsConfigCacheModel Get(int activityType)
        {
            var item = new UserPointsConfigCacheModel { ActivityType = activityType };

            return Get(item.HashField);
        }
    }
}

using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 用户经验值规则配置缓存
    /// </summary>
    public sealed class UserEmpiricalConfigCache : CacheItem<UserEmpiricalConfigCacheModel>
    {
        public UserEmpiricalConfigCache() : base(CacheItemType.UserEmpiricalConfig) { }

        protected override List<UserEmpiricalConfigCacheModel> GetCache()
        {
            List<UserEmpiricalConfigCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<UserEmpiricalConfigCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<UserEmpiricalConfigCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.UserEmpiricalConfigService.GetAll();
        }

        protected override void SetCache(List<UserEmpiricalConfigCacheModel> data)
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
        public override void Add(UserEmpiricalConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(UserEmpiricalConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(UserEmpiricalConfigCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override UserEmpiricalConfigCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<UserEmpiricalConfigCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="activityType">用户业务活动类型</param>
        /// <returns></returns>
        public UserEmpiricalConfigCacheModel Get(int activityType)
        {
            var item = new UserEmpiricalConfigCacheModel { ActivityType = activityType };

            return Get(item.HashField);
        }
    }
}

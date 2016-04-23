using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 圈子分类缓存
    /// </summary>
    public sealed class ForumCategoryCache : CacheItem<ForumCategoryCacheModel>
    {
        public ForumCategoryCache() : base(CacheItemType.ForumCategory) { }

        protected override List<ForumCategoryCacheModel> GetCache()
        {
            List<ForumCategoryCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<ForumCategoryCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<ForumCategoryCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.ForumCategoryService.GetEnabledAll();
        }

        protected override void SetCache(List<ForumCategoryCacheModel> data)
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
        public override void Add(ForumCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(ForumCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(ForumCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public override ForumCategoryCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<ForumCategoryCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="categoryID">圈子分类ID</param>
        /// <returns></returns>
        public ForumCategoryCacheModel Get(long categoryID)
        {
            var item = new ForumCategoryCacheModel { CategoryID = categoryID };

            return Get(item.HashField);
        }
    }
}

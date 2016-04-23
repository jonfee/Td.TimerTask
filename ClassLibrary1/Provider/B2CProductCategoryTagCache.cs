using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// B2C商品分类标签缓存
    /// </summary>
    public sealed class B2CProductCategoryTagCache : CacheItem<B2CProductCategoryTagCacheModel>
    {
        public B2CProductCategoryTagCache() : base(CacheItemType.B2CProductCategoryTags) { }

        protected override List<B2CProductCategoryTagCacheModel> GetCache()
        {
            List<B2CProductCategoryTagCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<B2CProductCategoryTagCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<B2CProductCategoryTagCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.B2CProductCategoryTagService.GetAll();
        }

        protected override void SetCache(List<B2CProductCategoryTagCacheModel> data)
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
        public override void Add(B2CProductCategoryTagCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(B2CProductCategoryTagCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(B2CProductCategoryTagCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override B2CProductCategoryTagCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<B2CProductCategoryTagCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="tagID">标签ID</param>
        /// <returns></returns>
        public B2CProductCategoryTagCacheModel Get(long tagID)
        {
            var item = new B2CProductCategoryTagCacheModel { TagID = tagID };

            return Get(item.HashField);
        }
    }
}

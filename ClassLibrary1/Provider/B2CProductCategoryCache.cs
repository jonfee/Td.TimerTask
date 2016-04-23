using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;
using System;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// B2C商品分类缓存
    /// </summary>
    public sealed class B2CProductCategoryCache : CacheItem<B2CProductCategoryCacheModel>
    {
        public B2CProductCategoryCache() : base(CacheItemType.B2CProductCategory) { }

        protected override List<B2CProductCategoryCacheModel> GetCache()
        {
            List<B2CProductCategoryCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<B2CProductCategoryCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<B2CProductCategoryCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.B2CProductCategoryService.GetEnabledAll();
        }

        protected override void SetCache(List<B2CProductCategoryCacheModel> data)
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
        public override void Add(B2CProductCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(B2CProductCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(B2CProductCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override B2CProductCategoryCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<B2CProductCategoryCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="categoryID">商品分类ID</param>
        /// <returns></returns>
        public B2CProductCategoryCacheModel Get(long categoryID)
        {
            var item = new B2CProductCategoryCacheModel { CategoryID = categoryID };

            return Get(item.HashField);
        }
    }
}

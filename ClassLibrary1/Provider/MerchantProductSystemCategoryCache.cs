using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 商家商品系统分类缓存
    /// </summary>
    public sealed class MerchantProductSystemCategoryCache : CacheItem<MerchantProductSystemCategoryCacheModel>
    {
        public MerchantProductSystemCategoryCache() : base(CacheItemType.MerchantProductSystemCategory) { }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Add(MerchantProductSystemCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(MerchantProductSystemCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(MerchantProductSystemCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        protected override List<MerchantProductSystemCategoryCacheModel> GetCache()
        {
            List<MerchantProductSystemCategoryCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<MerchantProductSystemCategoryCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<MerchantProductSystemCategoryCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.MerchantProductSystemCategoryService.GetEnabledAll();
        }

        protected override void SetCache(List<MerchantProductSystemCategoryCacheModel> data)
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
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override MerchantProductSystemCategoryCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<MerchantProductSystemCategoryCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="categoryID">商品分类ID</param>
        /// <returns></returns>
        public MerchantProductSystemCategoryCacheModel Get(long categoryID)
        {
            var item = new MerchantProductSystemCategoryCacheModel { CategoryID = categoryID };

            return Get(item.HashField);
        }
    }
}

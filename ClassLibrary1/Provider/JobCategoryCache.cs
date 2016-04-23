using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 职位类别缓存
    /// </summary>
    public sealed class JobCategoryCache : CacheItem<JobCategoryCacheModel>
    {
        public JobCategoryCache() : base(CacheItemType.JobCategory) { }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Add(JobCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(JobCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashDelete(CacheKey, entity.HashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="hashField">缓存中的HashField</param>
        /// <returns></returns>
        public override JobCategoryCacheModel Get(string hashField)
        {
            return RedisDB.HashGet<JobCategoryCacheModel>(CacheKey, hashField);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="categoryID">分类ID</param>
        /// <returns></returns>
        public JobCategoryCacheModel Get(long categoryID)
        {
            var item = new JobCategoryCacheModel { CategoryID = categoryID };

            return Get(item.HashField);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(JobCategoryCacheModel entity)
        {
            if (null == entity) return;

            RedisDB.HashSetAsync(CacheKey, entity.HashField, entity);
        }

        protected override List<JobCategoryCacheModel> GetCache()
        {
            List<JobCategoryCacheModel> data = null;

            if (null != RedisDB)
            {
                data = RedisDB.HashGetAll<JobCategoryCacheModel>(CacheKey).Select(p => p.Value).ToList();
            }

            return data;
        }

        protected override List<JobCategoryCacheModel> ReadDataFromDB()
        {
            return ServicesProvider.Items.JobCategoryService.GetAll();
        }

        protected override void SetCache(List<JobCategoryCacheModel> data)
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

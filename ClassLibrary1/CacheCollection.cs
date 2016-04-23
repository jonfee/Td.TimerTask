using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.Provider;

namespace Td.Kylin.DataCache
{
    /// <summary>
    /// 缓存采集器
    /// </summary>
    public sealed class CacheCollection
    {
        /// <summary>
        /// 缓存项实例集合
        /// </summary>
        private static Hashtable htCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 静态构造
        /// </summary>
        static CacheCollection()
        {
            htCache = Hashtable.Synchronized(new Hashtable());

            var configCollections = CacheStartup.RedisConfiguration.Collections;

            foreach (var config in configCollections)
            {
                if (null != config)
                {
                    var cacheItem = CacheItemFactory(config.ItemType);

                    if (null != cacheItem)
                    {
                        htCache.Add(config.RedisKey, cacheItem);
                    }
                }
            }
        }

        /// <summary>
        /// 缓存数量
        /// </summary>
        public static int Count
        {
            get
            {
                return null != htCache ? htCache.Count : 0;
            }
        }

        /// <summary>
        /// 缓存的Key集合
        /// </summary>
        public static string[] Keys
        {
            get
            {
                var arr = new ArrayList(htCache.Keys);

                List<string> keys = new List<string>();

                foreach (var key in arr)
                {
                    var str = key as string;

                    if (null != str)
                    {
                        keys.Add(str);
                    }
                }

                keys.TrimExcess();

                return keys.ToArray();
            }
        }

        /// <summary>
        /// 缓存项生成
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        static dynamic CacheItemFactory(CacheItemType itemType)
        {
            dynamic cacheItem = null;

            switch (itemType)
            {
                //系统区域缓存
                case CacheItemType.SystemArea:
                    cacheItem = new SystemAreaCache();
                    break;
                //开通区域缓存
                case CacheItemType.OpenArea:
                    cacheItem = new OpenAreaCache();
                    break;
                //商家行业缓存
                case CacheItemType.MerchantIndustry:
                    cacheItem = new MerchantIndustryCache();
                    break;
                //区域行业推荐缓存
                case CacheItemType.AreaRecommendIndustry:
                    cacheItem = new AreaRecommendIndustryCache();
                    break;
                //上门预约业务缓存
                case CacheItemType.BusinessServices:
                    cacheItem = new BusinessServiceCache();
                    break;
                //B2C商品分类
                case CacheItemType.B2CProductCategory:
                    cacheItem = new B2CProductCategoryCache();
                    break;
                //B2C商品分类标签
                case CacheItemType.B2CProductCategoryTags:
                    cacheItem = new B2CProductCategoryTagCache();
                    break;
                //全局配置
                case CacheItemType.SystemGolbalConfig:
                    cacheItem = new SystemGolbalConfigCache();
                    break;
                //用户积分规则配置 
                case CacheItemType.UserPointsConfig:
                    cacheItem = new UserPointsConfigCache();
                    break;
                //用户经验值规则配置 
                case CacheItemType.UserEmpiricalConfig:
                    cacheItem = new UserEmpiricalConfigCache();
                    break;
                //用户等级配置 
                case CacheItemType.UserLevelConfig:
                    cacheItem = new UserLevelConfigCache();
                    break;
                //圈子分类
                case CacheItemType.ForumCategory:
                    cacheItem = new ForumCategoryCache();
                    break;
                //系统圈子
                case CacheItemType.ForumCircle:
                    cacheItem = new ForumCircleCache();
                    break;
                //区域圈子
                case CacheItemType.AreaForum:
                    cacheItem = new AreaForumCache();
                    break;
                //商家商品系统分类
                case CacheItemType.MerchantProductSystemCategory:
                    cacheItem = new MerchantProductSystemCategoryCache();
                    break;
                //职位类别
                case CacheItemType.JobCategory:
                    cacheItem = new JobCategoryCache();
                    break;
            }

            return cacheItem;
        }

        #region 缓存实例及值

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static T GetCacheObject<T>(CacheItemType itemType)
        {
            CacheConfig config = CacheStartup.RedisConfiguration[itemType];

            string cacheKey = config?.RedisKey;

            if (Keys.Contains(cacheKey))
            {
                object cache = htCache[cacheKey];

                if (cache is T)
                {
                    return (T)cache;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 系统区域缓存
        /// </summary>
        public static SystemAreaCache SystemAreaCache { get { return GetCacheObject<SystemAreaCache>(CacheItemType.SystemArea); } }

        /// <summary>
        /// 开通区域缓存
        /// </summary>
        public static OpenAreaCache OpenAreaCache { get { return GetCacheObject<OpenAreaCache>(CacheItemType.OpenArea); } }

        /// <summary>
        /// 商家行业缓存
        /// </summary>
        public static MerchantIndustryCache MerchantIndustryCache { get { return GetCacheObject<MerchantIndustryCache>(CacheItemType.MerchantIndustry); } }

        /// <summary>
        /// 区域行业推荐缓存
        /// </summary>
        public static AreaRecommendIndustryCache AreaRecommendIndustryCache { get { return GetCacheObject<AreaRecommendIndustryCache>(CacheItemType.AreaRecommendIndustry); } }

        /// <summary>
        /// 上门预约服务缓存
        /// </summary>
        public static BusinessServiceCache BusinessServiceCache { get { return GetCacheObject<BusinessServiceCache>(CacheItemType.BusinessServices); } }

        /// <summary>
        /// 商家商品系统分类缓存
        /// </summary>
        public static MerchantProductSystemCategoryCache MerchantProductSystemCategoryCache { get { return GetCacheObject<MerchantProductSystemCategoryCache>(CacheItemType.MerchantProductSystemCategory); } }

        /// <summary>
        /// B2C商品分类缓存
        /// </summary>
        public static B2CProductCategoryCache B2CProductCategoryCache { get { return GetCacheObject<B2CProductCategoryCache>(CacheItemType.B2CProductCategory); } }

        /// <summary>
        /// B2C商品分类标签缓存
        /// </summary>
        public static B2CProductCategoryTagCache B2CProductCategoryTagCache { get { return GetCacheObject<B2CProductCategoryTagCache>(CacheItemType.B2CProductCategoryTags); } }

        /// <summary>
        /// 系统全局配置缓存
        /// </summary>
        public static SystemGolbalConfigCache SystemGolbalConfigCache { get { return GetCacheObject<SystemGolbalConfigCache>(CacheItemType.SystemGolbalConfig); } }

        /// <summary>
        /// 用户积分规则配置缓存 
        /// </summary>
        public static UserPointsConfigCache UserPointsConfigCache { get { return GetCacheObject<UserPointsConfigCache>(CacheItemType.UserPointsConfig); } }

        /// <summary>
        /// 用户经验值规则配置缓存
        /// </summary>
        public static UserEmpiricalConfigCache UserEmpiricalConfigCache { get { return GetCacheObject<UserEmpiricalConfigCache>(CacheItemType.UserEmpiricalConfig); } }

        /// <summary>
        /// 用户等级配置缓存
        /// </summary>
        public static UserLevelConfigCache UserLevelConfigCache { get { return GetCacheObject<UserLevelConfigCache>(CacheItemType.UserLevelConfig); } }

        /// <summary>
        /// 圈子分类缓存
        /// </summary>
        public static ForumCategoryCache ForumCategoryCache { get { return GetCacheObject<ForumCategoryCache>(CacheItemType.ForumCategory); } }

        /// <summary>
        /// 系统圈子缓存
        /// </summary>
        public static ForumCircleCache ForumCircleCache { get { return GetCacheObject<ForumCircleCache>(CacheItemType.ForumCircle); } }

        /// <summary>
        /// 区域圈子缓存
        /// </summary>
        public static AreaForumCache AreaForumCache { get { return GetCacheObject<AreaForumCache>(CacheItemType.AreaForum); } }

        /// <summary>
        /// 职位类别缓存
        /// </summary>
        public static JobCategoryCache JobCategoryCache { get { return GetCacheObject<JobCategoryCache>(CacheItemType.JobCategory); } }

        #endregion
    }
}

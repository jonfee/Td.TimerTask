using Td.Kylin.DataCache.RedisConfig;

namespace Td.Kylin.DataCache
{
    internal sealed class CacheStartup
    {
        static CacheStartup()
        {
            RedisConfiguration = InitRedisConfigration();
        }
        
        /// <summary>
        /// Redis缓存配置
        /// </summary>
        public static RedisConfigurationRoot RedisConfiguration { get;private set; }

        /// <summary>
        /// 数据库提供者类型
        /// </summary>
        public static SqlProviderType SqlType { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string SqlConnctionString { get; set; }

        #region 初始化Redis配置

        /// <summary>
        /// 创建并初始化Redis缓存配置
        /// </summary>
        /// <returns></returns>
        private static RedisConfigurationRoot InitRedisConfigration()
        {
            var config = new RedisConfigurationRoot();

            //全国区域
            config.Add(CacheItemType.SystemArea, 0, RedisSaveType.HashSet,CacheLevel.Permanent);
            //开通区域
            config.Add(CacheItemType.OpenArea, 0, RedisSaveType.HashSet,CacheLevel.Hight);
            //区域圈子
            config.Add(CacheItemType.AreaForum, 0, RedisSaveType.HashSet,CacheLevel.Hight);
            //区域行业推荐
            config.Add(CacheItemType.AreaRecommendIndustry, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //B2C商品分类
            config.Add(CacheItemType.B2CProductCategory, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //B2C商品分类下标签
            config.Add(CacheItemType.B2CProductCategoryTags, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //上门预约服务业务
            config.Add(CacheItemType.BusinessServices, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //圈子分类
            config.Add(CacheItemType.ForumCategory, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //系统圈子
            config.Add(CacheItemType.ForumCircle, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //商家行业
            config.Add(CacheItemType.MerchantIndustry, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //商家商品系统分类
            config.Add(CacheItemType.MerchantProductSystemCategory, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //全局配置
            config.Add(CacheItemType.SystemGolbalConfig, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //用户经验值规则配置
            config.Add(CacheItemType.UserEmpiricalConfig, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //用户等级规则配置
            config.Add(CacheItemType.UserLevelConfig, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //用户积分规则配置
            config.Add(CacheItemType.UserPointsConfig, 0, RedisSaveType.HashSet, CacheLevel.Hight);
            //职位类别
            config.Add(CacheItemType.JobCategory, 0, RedisSaveType.HashSet, CacheLevel.Hight);

            return config;
        }

        #endregion
    }
}

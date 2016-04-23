using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;
using Td.Kylin.DataCache.Services;

namespace Td.Kylin.DataCache
{
    internal sealed class ServicesProvider
    {
        private static ServicesProvider _instance;

        private static readonly object myLock = new object();

        /// <summary>
        /// 数据操作服务集
        /// </summary>
        public static ServicesProvider Items
        {
            get
            {
                if (null == _instance)
                {
                    lock (myLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServicesProvider();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        private ServicesProvider()
        {
            switch (CacheStartup.SqlType)
            {
                case SqlProviderType.PostgreSQL:
                    InitServices<PostgreSqlDataContext>();
                    break;
                case SqlProviderType.SqlServer:
                    InitServices<SqlServerDataContext>();
                    break;
            }
        }

        #region 数据操作服务定义

        /// <summary>
        /// 系统区域数据服务
        /// </summary>
        public ISystemAreaService SystemAreaService { get; private set; }

        /// <summary>
        /// 开通区域数据服务
        /// </summary>
        public IOpenAreaService OpenAreaService { get; private set; }

        /// <summary>
        /// 商家行业数据服务
        /// </summary>
        public IMerchantIndustryService MerchantIndustryService { get; private set; }

        /// <summary>
        /// 区域行业推荐数据服务
        /// </summary>
        public IAreaRecommendIndustryService AreaRecommendIndustryService { get; private set; }

        /// <summary>
        /// 上门预约业务数据服务
        /// </summary>
        public IBusinessService BusinessService { get; private set; }

        /// <summary>
        /// B2C区域商城商品分类
        /// </summary>
        public IB2CProductCategoryService B2CProductCategoryService { get; private set; }

        /// <summary>
        /// B2C区域商城商品分类标签
        /// </summary>
        public IB2CProductCategoryTagService B2CProductCategoryTagService { get; private set; }

        /// <summary>
        /// 系统全局配置
        /// </summary>
        public ISystemGolbalConfigService SystemGolbalConfigService { get; private set; }

        /// <summary>
        /// 用户积分规则配置 
        /// </summary>
        public IUserPointsConfigService UserPointsConfigService { get; private set; }

        /// <summary>
        /// 用户经验值规则配置 
        /// </summary>
        public IUserEmpiricalConfigService UserEmpiricalConfigService { get; private set; }

        /// <summary>
        /// 用户等级配置
        /// </summary>
        public IUserLevelConfigService UserLevelConfigService { get; private set; }

        /// <summary>
        /// 圈子分类
        /// </summary>
        public IForumCategoryService ForumCategoryService { get; private set; }

        /// <summary>
        /// 系统圈子
        /// </summary>
        public IForumCircleService ForumCircleService { get; private set; }

        /// <summary>
        /// 区域圈子
        /// </summary>
        public IAreaForumService AreaForumService { get; private set; }

        /// <summary>
        /// 商家商品系统分类
        /// </summary>
        public IMerchantProductSystemCategoryService MerchantProductSystemCategoryService { get; private set; }

        /// <summary>
        /// 职位类别
        /// </summary>
        public IJobCategoryService JobCategoryService { get; private set; }

        #endregion

        /// <summary>
        /// 数据服务初始化
        /// </summary>
        /// <typeparam name="T"></typeparam>

        void InitServices<T>() where T : DataContext, new()
        {
            //系统区域
            SystemAreaService = new SystemAreaService<T>();
            //开通区域
            OpenAreaService = new OpenAreaService<T>();
            //商家行业
            MerchantIndustryService = new MerchantIndustryService<T>();
            //区域行业推荐
            AreaRecommendIndustryService = new AreaRecommendIndustryService<T>();
            //上门预约业务
            BusinessService = new BusinessService<T>();
            //B2C商品分类
            B2CProductCategoryService = new B2CProductCategoryService<T>();
            //B2C区域商城商品分类标签
            B2CProductCategoryTagService = new B2CProductCategoryTagService<T>();
            //系统全局配置
            SystemGolbalConfigService = new SystemGolbalConfigService<T>();
            //用户积分规则配置
            UserPointsConfigService = new UserPointsConfigService<T>();
            //用户经验值规则配置 
            UserEmpiricalConfigService = new UserEmpiricalConfigService<T>();
            //用户等级配置
            UserLevelConfigService = new UserLevelConfigService<T>();
            //圈子分类
            ForumCategoryService = new ForumCategoryService<T>();
            //系统圈子
            ForumCircleService = new ForumCircleService<T>();
            //区域圈子
            AreaForumService = new AreaForumService<T>();
            //商家商品系统分类
            MerchantProductSystemCategoryService = new MerchantProductSystemCategoryService<T>();
            //职位类别
            JobCategoryService = new JobCategoryService<T>();
        }
    }
}

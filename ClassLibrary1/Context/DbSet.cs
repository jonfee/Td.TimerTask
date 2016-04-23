using Microsoft.Data.Entity;
using Td.Kylin.Entity;

namespace Td.Kylin.DataCache.Context
{
    internal partial class DataContext
    {
        /// <summary>
        /// 系统模块接口授权
        /// </summary>
        public DbSet<System_ModuleAuthorize> System_ModuleAuthorize { get { return Set<System_ModuleAuthorize>(); } }

        public DbSet<System_GlobalResources> System_GlobalResources { get { return Set<System_GlobalResources>(); } }

        public DbSet<System_PointsConfig> System_PointsConfig { get { return Set<System_PointsConfig>(); } }

        public DbSet<System_Level> System_Level { get { return Set<System_Level>(); } }

        public DbSet<System_EmpiricalConfig> System_EmpiricalConfig { get { return Set<System_EmpiricalConfig>(); } }

        public DbSet<Merchant_Industry> Merchant_Industry { get { return Set<Merchant_Industry>(); } }

        public DbSet<Job_Category> Job_Category { get { return Set<Job_Category>(); } }

        public DbSet<KylinService_Business> KylinService_Business { get { return Set<KylinService_Business>(); } }

        public DbSet<KylinService_BusinessOptions> KylinService_BusinessOptions { get { return Set<KylinService_BusinessOptions>(); } }

        public DbSet<KylinService_BusinessConfig> KylinService_BusinessConfig { get { return Set<KylinService_BusinessConfig>(); } }

        public DbSet<System_Area> System_Area { get { return Set<System_Area>(); } }

        public DbSet<Area_Open> Area_Open { get { return Set<Area_Open>(); } }

        public DbSet<Area_RecommendIndustry> Area_RecommendIndustry { get { return Set<Area_RecommendIndustry>(); } }

        public DbSet<Circle_Category> Circle_Category { get { return Set<Circle_Category>(); } }

        public DbSet<Circle_Forum> Circle_Forum { get { return Set<Circle_Forum>(); } }

        public DbSet<Circle_AreaForum> Circle_AreaForum { get { return Set<Circle_AreaForum>(); } }

        /// <summary>
        /// 商品分类
        /// </summary>
        public DbSet<Mall_Category> Mall_Category { get { return Set<Mall_Category>(); } }

        /// <summary>
        /// 商品分类标签
        /// </summary>
        public DbSet<Mall_CategoryTag> Mall_CategoryTag { get { return Set<Mall_CategoryTag>(); } }

        /// <summary>
        /// 商家商品系统分类
        /// </summary>
        public DbSet<MerchantGoods_SystemCategory> MerchantGoods_SystemCategory { get { return Set<MerchantGoods_SystemCategory>(); } }
    }
}

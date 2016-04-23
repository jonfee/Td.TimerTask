using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Td.Kylin.Entity;

namespace Td.Kylin.DataCache.Context
{
    internal abstract partial class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseSqlServer(CacheStartup.SqlConnctionString);
        }

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //系统模块接口授权
            modelBuilder.Entity<System_ModuleAuthorize>(entity =>
            {
                entity.HasKey(p => new { p.ServerID, p.ModuleID });
            });

            //系统全局配置
            modelBuilder.Entity<System_GlobalResources>(entity =>
            {
                entity.HasKey(p => new { p.ResourceType, p.ResourceKey });
            });

            //积分配置
            modelBuilder.Entity<System_PointsConfig>(entity =>
            {
                entity.Property(p => p.ActivityType).ValueGeneratedNever();
                entity.HasKey(p => p.ActivityType);
            });

            //系统用户等级配置
            modelBuilder.Entity<System_Level>(entity =>
            {
                entity.Property(p => p.LevelID).ValueGeneratedNever();
                entity.HasKey(p => p.LevelID);
            });

            //经验值配置
            modelBuilder.Entity<System_EmpiricalConfig>(entity =>
            {
                entity.Property(p => p.ActivityType).ValueGeneratedNever();
                entity.HasKey(p => p.ActivityType);
            });

            //系统商家行业
            modelBuilder.Entity<Merchant_Industry>(entity =>
            {
                entity.Property(p => p.IndustryID).ValueGeneratedNever();
                entity.HasKey(p => p.IndustryID);
            });

            //岗位
            modelBuilder.Entity<Job_Category>(entity =>
            {
                entity.Property(p => p.CategoryID).ValueGeneratedNever();
                entity.HasKey(p => p.CategoryID);
            });

            //上门预约业务
            modelBuilder.Entity<KylinService_Business>(entity =>
            {
                entity.Property(p => p.BusinessID).ValueGeneratedNever();
                entity.HasKey(p => p.BusinessID);
            });

            //上门预约业务子项
            modelBuilder.Entity<KylinService_BusinessOptions>(entity =>
            {
                entity.Property(p => p.OptionID).ValueGeneratedNever();
                entity.HasKey(p => p.OptionID);
            });

            //上门预约业务配置
            modelBuilder.Entity<KylinService_BusinessConfig>(entity =>
            {
                entity.HasKey(p => new { p.BusinessID, p.OptionID });
            });

            //系统区域
            modelBuilder.Entity<System_Area>(entity =>
            {
                entity.Property(p => p.AreaID).ValueGeneratedNever();
                entity.HasKey(p => p.AreaID);
            });

            //开通区域
            modelBuilder.Entity<Area_Open>(entity =>
            {
                entity.Property(p => p.AreaID).ValueGeneratedNever();
                entity.HasKey(p => p.AreaID);
            });

            //区域行业推荐
            modelBuilder.Entity<Area_RecommendIndustry>(entity =>
            {
                entity.HasKey(p => new { p.AreaID, p.IndustryID });
            });

            //圈子分类
            modelBuilder.Entity<Circle_Category>(entity =>
            {
                entity.Property(p => p.CategoryID).ValueGeneratedNever();
                entity.HasKey(p => p.CategoryID);
            });

            //系统圈子
            modelBuilder.Entity<Circle_Forum>(entity =>
            {
                entity.Property(p => p.ForumID).ValueGeneratedNever();
                entity.HasKey(p => p.ForumID);
            });

            //区域圈子
            modelBuilder.Entity<Circle_AreaForum>(entity =>
            {
                entity.Property(p => p.AreaForumID).ValueGeneratedNever();
                entity.HasKey(p => p.AreaForumID);
            });
            
            modelBuilder.Entity<Mall_Category>(entity =>
            {
                entity.Property(p => p.CategoryID).ValueGeneratedNever();
                entity.HasKey(p => p.CategoryID);
            });

            modelBuilder.Entity<Mall_CategoryTag>(entity =>
            {
                entity.Property(p => p.TagID).ValueGeneratedNever();
                entity.HasKey(p => p.TagID);
            });

            modelBuilder.Entity<MerchantGoods_SystemCategory>(entity =>
            {
                entity.Property(p => p.CategoryID).ValueGeneratedNever();
                entity.HasKey(p => p.CategoryID);
            });
        }

        #endregion
    }
}

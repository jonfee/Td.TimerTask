using KylinService.Core;
using KylinService.Data.Entity;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KylinService.Data
{
    public class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseSqlServer(Configs.KylinConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //用户账户
            modelBuilder.Entity<User_Account>(entity =>
            {
                entity.Property(p => p.UserID).ValueGeneratedNever();
                entity.HasKey(p => p.UserID);
            });

            //用户摇一摇信息
            modelBuilder.Entity<User_ShakeRecord>(entity =>
            {
                entity.Property(p => p.UserID).ValueGeneratedNever();
                entity.HasKey(p => p.UserID);
            });

            //商家福利
            modelBuilder.Entity<Merchant_Welfare>(entity =>
            {
                entity.Property(p => p.WelfareID).ValueGeneratedNever();
                entity.HasKey(p => p.WelfareID);
            });

            //福利分期活动
            modelBuilder.Entity<Welfare_Phases>(entity =>
            {
                entity.Property(p => p.PhasesID).ValueGeneratedNever();
                entity.HasKey(p => p.PhasesID);
            });

            //福利报名参与人员
            modelBuilder.Entity<Welfare_PartUser>(entity =>
            {
                entity.HasKey(p => new { p.PhasesID, p.UserID });
            });

            //用户福利
            modelBuilder.Entity<User_Welfare>(entity =>
            {
                entity.Property(p => p.ConsumerCode).ValueGeneratedNever();
                entity.HasKey(p => p.ConsumerCode);
            });
        }

        #region DbSet

        /// <summary>
        /// 用户账户
        /// </summary>
        public DbSet<User_Account> User_Account { get { return Set<User_Account>(); } }

        /// <summary>
        /// 用户摇一摇信息
        /// </summary>
        public DbSet<User_ShakeRecord> User_ShakeRecord { get { return Set<User_ShakeRecord>(); } }

        /// <summary>
        /// 商家福利
        /// </summary>
        public DbSet<Merchant_Welfare> Merchant_Welfare { get { return Set<Merchant_Welfare>(); } }

        /// <summary>
        /// 福利分期活动
        /// </summary>
        public DbSet<Welfare_Phases> Welfare_Phases { get { return Set<Welfare_Phases>(); } }

        /// <summary>
        /// 福利报名参与人员
        /// </summary>
        public DbSet<Welfare_PartUser> Welfare_PartUser { get { return Set<Welfare_PartUser>(); } }

        /// <summary>
        /// 用户福利
        /// </summary>
        public DbSet<User_Welfare> User_Welfare { get { return Set<User_Welfare>(); } }

        #endregion
    }
}

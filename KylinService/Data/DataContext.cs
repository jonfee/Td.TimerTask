using KylinService.Core;
using KylinService.Data.Entity;
using Microsoft.Data.Entity;

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

            //用户交易记录
            modelBuilder.Entity<User_TradeRecords>(entity =>
            {
                entity.Property(p => p.TradeID).ValueGeneratedNever();
                entity.HasKey(p => p.TradeID);
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

            //商城订单
            modelBuilder.Entity<Mall_Order>(entity =>
            {
                entity.Property(p => p.OrderID).ValueGeneratedNever();
                entity.HasKey(p => p.OrderID);
            });

            //系统业务
            modelBuilder.Entity<KylinService_Business>(entity =>
            {
                entity.Property(p => p.BusinessID).ValueGeneratedNever();
                entity.HasKey(p => p.BusinessID);
            });

            //业务订单
            modelBuilder.Entity<KylinService_Order>(entity =>
            {
                entity.Property(p => p.OrderID).ValueGeneratedNever();
                entity.HasKey(p => p.OrderID);
            });

            //商家账户
            modelBuilder.Entity<Merchant_Account>(entity =>
            {
                entity.Property(p => p.MerchantID).ValueGeneratedNever();
                entity.HasKey(p => p.MerchantID);
            });

            //商家交易记录
            modelBuilder.Entity<Merchant_TradeRecords>(entity =>
            {
                entity.Property(p => p.TradeID).ValueGeneratedNever();
                entity.HasKey(p => p.TradeID);
            });

            //服务人员账号
            modelBuilder.Entity<Worker_Account>(entity =>
            {
                entity.Property(p => p.WorkerID).ValueGeneratedNever();
                entity.HasKey(p => p.WorkerID);
            });

            //服务人员交易记录
            modelBuilder.Entity<Worker_TradeRecords>(entity =>
            {
                entity.Property(p => p.TradeID).ValueGeneratedNever();
                entity.HasKey(p => p.TradeID);
            });
            
        }

        #region DbSet

        /// <summary>
        /// 用户账户
        /// </summary>
        public DbSet<User_Account> User_Account { get { return Set<User_Account>(); } }

        /// <summary>
        /// 用户交易记录
        /// </summary>
        public DbSet<User_TradeRecords> User_TradeRecords { get { return Set<User_TradeRecords>(); } }

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

        /// <summary>
        /// 商城订单
        /// </summary>
        public DbSet<Mall_Order> Mall_Order { get { return Set<Mall_Order>(); } }

        /// <summary>
        /// 系统业务
        /// </summary>
        public DbSet<KylinService_Business> KylinService_Business { get { return Set<KylinService_Business>(); } }

        /// <summary>
        /// 业务订单
        /// </summary>
        public DbSet<KylinService_Order> KylinService_Order { get { return Set<KylinService_Order>(); } }

        /// <summary>
        /// 商家交易记录
        /// </summary>
        public DbSet<Merchant_TradeRecords> Merchant_TradeRecords { get { return Set<Merchant_TradeRecords>(); } }

        /// <summary>
        /// 商家账户信息
        /// </summary>
        public DbSet<Merchant_Account> Merchant_Account { get { return Set<Merchant_Account>(); } }

        /// <summary>
        /// 服务职员账户
        /// </summary>
        public DbSet<Worker_Account> Worker_Account { get { return Set<Worker_Account>(); } }

        /// <summary>
        /// 服务职员交易记录
        /// </summary>
        public DbSet<Worker_TradeRecords> Worker_TradeRecords { get { return Set<Worker_TradeRecords>(); } }

        #endregion
    }
}

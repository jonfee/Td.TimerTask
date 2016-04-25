using KylinService.Core;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Td.Kylin.DataCache;

namespace KylinService
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();

            InitializeConfig();
        }

        void InitializeConfig()
        {
            //数据库信息
            ShowDatabaseConnection();

            //任务计划数据在Redis缓存中的配置
            ShowScheduleRedisConfig();

            //推送消息 Redis配置信息
            ShowPushContentRedisConfig();

            //社区配置信息
            ShowCircleConfig();

            //福利配置信息
            ShowWelfareConfig();

            //上门预约订单配置信息
            ShowAppointConfig();

            //精品汇订单配置信息
            ShowMallConfig();

            //附近购订单配置信息
            ShowMerchantConfig();

            //缓存维护参数配置信息
            ShowCacheMaintainConfig();
        }

        /// <summary>
        /// 显示数据库信息
        /// </summary>
        void ShowDatabaseConnection()
        {
            string conn = Startup.KylinDBConnectionString;

            Regex regServer = new Regex(@"(server|host|data source)=(?<server>[^;""’’]+)", RegexOptions.IgnoreCase);
            Regex regDataBase = new Regex(@"(database|initial catalog)=(?<database>[^;""]+)", RegexOptions.IgnoreCase);

            string server = null;
            string database = null;

            var scoll = regServer.Matches(conn);
            if (null != scoll)
            {
                Match m = scoll[0];
                server = m.Groups["server"].Value;
            }

            var dcoll = regDataBase.Match(conn);
            if (null != dcoll)
            {
                database = dcoll.Groups["database"].Value;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("当前数据库信息：");
            sb.AppendLine(string.Format("   服务器：{0}", server));
            sb.AppendLine(string.Format("   数据库名：{0}", database));

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 任务计划数据在Redi中的配置信息
        /// </summary>
        void ShowScheduleRedisConfig()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var config in Startup.ScheduleRedisConfigs.Items)
            {
                var servName = SysData.GetQueueServiceName(config.ScheduleName);
                sb.AppendLine(string.Format("任务名：{0}", servName));

                string server = null;
                Regex regServer = new Regex(@"(?<server>[0-9a-z\-]+(\.[0-9a-z\-]+)+)", RegexOptions.IgnoreCase);
                var dcoll = regServer.Match(config.ConnectionString);
                if (null != dcoll)
                {
                    server = dcoll.Groups["server"].Value;
                }
                sb.AppendLine(string.Format("   Redis服务器：{0}", server));
                sb.AppendLine(string.Format("   Redis存储数据库序号：{0}", config.DbIndex));
                sb.AppendLine(string.Format("   Redis存储Key：{0}", config.Key));
                sb.AppendLine();
            }

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 推送消息 Redis配置信息
        /// </summary>
        void ShowPushContentRedisConfig()
        {
            StringBuilder sb = new StringBuilder();

            var dic = typeof(RedisPushType).GetNameDescription() ?? new Dictionary<string, string>();

            foreach (var config in Startup.PushRedisConfigs.Items)
            {
                var servName = dic.ContainsKey(config.SaveType.ToString()) ? dic[config.SaveType.ToString()] : string.Empty;

                sb.AppendLine(string.Format("推送类型：{0}", servName));

                string server = null;
                Regex regServer = new Regex(@"(?<server>[0-9a-z\-]+(\.[0-9a-z\-]+)+)", RegexOptions.IgnoreCase);
                var dcoll = regServer.Match(config.ConnectionString);
                if (null != dcoll)
                {
                    server = dcoll.Groups["server"].Value;
                }
                sb.AppendLine(string.Format("   Redis服务器：{0}", server));
                sb.AppendLine(string.Format("   Redis存储数据库序号：{0}", config.DbIndex));
                sb.AppendLine(string.Format("   Redis存储Key：{0}", config.Key));
                sb.AppendLine();
            }

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 社区配置信息
        /// </summary>
        void ShowCircleConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("社区自助服务配置：");
            sb.AppendLine(string.Format("   活动开始前{0}分钟提醒", Startup.CircleConfig.BeforeRemindMinutes));

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 福利配置信息
        /// </summary>
        void ShowWelfareConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("福利自助服务配置：");
            sb.AppendLine(string.Format("   福利开放前{0}分钟提醒", Startup.WelfareConfig.BeforeRemindMinutes));

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 上门预约订单配置信息
        /// </summary>
        void ShowAppointConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("上门/预约自助服务配置：");
            sb.AppendLine(string.Format("   未支付订单超过{0}分钟系统自动取消订单", Startup.AppointConfig.PaymentWaitMinutes));
            sb.AppendLine(string.Format("   服务结束后超过{0}天系统自动确认服务完成", Startup.AppointConfig.EndServiceWaitUserDays));

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 精品汇订单配置信息
        /// </summary>
        void ShowMallConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("精品汇（B2C）自助服务配置：");
            sb.AppendLine(string.Format("   未支付订单超过{0}分钟系统自动取消订单", Startup.B2COrderConfig.WaitPaymentMinutes));
            sb.AppendLine(string.Format("   发货后超过{0}天系统自动确认服务完成", Startup.B2COrderConfig.WaitReceiptGoodsDays));

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 附近购订单配置信息
        /// </summary>
        void ShowMerchantConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("附近购（商家）自助服务配置：");
            sb.AppendLine(string.Format("   未支付订单超过{0}分钟系统自动取消订单", Startup.MerchantOrderConfig.WaitPaymentMinutes));
            sb.AppendLine(string.Format("   发货后超过{0}天系统自动确认服务完成", Startup.MerchantOrderConfig.WaitReceiptGoodsDays));

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 缓存维护参数配置信息
        /// </summary>
        void ShowCacheMaintainConfig()
        {
            StringBuilder sb = new StringBuilder();

            if (null != Startup.CacheMaintainConfigs && Startup.CacheMaintainConfigs.Count > 0)
            {
                sb.AppendLine("缓存维护参数配置：");

                foreach (var config in Startup.CacheMaintainConfigs)
                {
                    string levelDesc = "";

                    switch (config.Level)
                    {
                        case CacheLevel.Hight: levelDesc = "高"; break;
                        case CacheLevel.Lower: levelDesc = "低"; break;
                        case CacheLevel.Middel: levelDesc = "中等"; break;
                        case CacheLevel.Permanent: levelDesc = "持久"; break;
                    }

                    string option = SysData.GetCacheTimeOptionName(config.TimeOption.ToString());

                    sb.AppendLine(string.Format("   {0}级别的缓存更新周期为 {1} {2}", levelDesc, config.PeriodTime, option));
                }
            }

            WriteOutConfig(sb.ToString(), true);
        }

        /// <summary>
        /// 输出配置信息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="addSpliter">是否在内容前加分隔行</param>
        void WriteOutConfig(string content, bool addSpliteLine)
        {
            if (addSpliteLine)
            {
                this.rtContent.AppendText("\n");
                this.rtContent.AppendText("------------------------------------------");
                this.rtContent.AppendText("\n");
            }
            this.rtContent.AppendText(content);
            this.rtContent.AppendText("\n");
            this.rtContent.Focus();
        }
    }
}

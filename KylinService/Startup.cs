﻿using KylinService.Core;
using KylinService.Redis.Push;
using KylinService.Redis.Schedule;
using KylinService.Services.CacheMaintain;
using KylinService.Services.Queue.Appoint;
using KylinService.Services.Queue.Circle;
using KylinService.Services.Queue.Mall;
using KylinService.Services.Queue.Merchant;
using KylinService.Services.Queue.Welfare;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Td.Kylin.DataCache;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.EnumLibrary;

namespace KylinService
{
    internal sealed class Startup
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            #region//产品信息及维护人员信息
            ProductInfo = new ProductInfo
            {
                Author = ConfigurationManager.AppSettings["Author"],
                Email = ConfigurationManager.AppSettings["Email"],
                Mobile = ConfigurationManager.AppSettings["Mobile"],
                QQ = ConfigurationManager.AppSettings["QQ"],
                ProductName = Application.ProductName,
                Version = Application.ProductVersion
            };
            #endregion

            #region //Kylin数据库类型及连接字符串

            SqlType = new Func<SqlProviderType>(() =>
              {
                  string sqlType = ConfigurationManager.AppSettings["SqlType"] ?? string.Empty;
                  switch (sqlType.ToLower())
                  {
                      case "npgsql":
                          return SqlProviderType.NpgSQL;
                      case "mssql":
                      default:
                          return SqlProviderType.SqlServer;
                  }

              }).Invoke();

            KylinDBConnectionString = ConfigurationManager.ConnectionStrings["KylinConnectionString"].ConnectionString;

            #endregion

            #region  //注入

            DataCacheRedisConnectionString = ConfigurationManager.ConnectionStrings["RedisDataCacheConnectionString"].ConnectionString;
            //注入数据缓存组件
            InjectionDataCache();

            #endregion

            #region //任务计划数据在Redis中的配置
            ScheduleRedisConfigs = ScheduleConfigManager.Collection;
            #endregion

            #region //推送消息 Redis配置
            PushRedisConfigs = PushRedisConfigManager.Collection;
            #endregion

            //更新队列服务配置
            UpdateQueueConfig();

            //初始化缓存维护参数配置
            UpdateCacheMaintainConfig();
        }

        /// <summary>
        /// 更新任务计划的队列Redis服务器
        /// </summary>
        /// <param name="scheduleRedisConn"></param>
        public static void UpdateScheduleRedis(string scheduleRedisConn)
        {
            if (null != ScheduleRedisConfigs)
            {
                foreach (var item in ScheduleRedisConfigs.Items)
                {
                    item.ConnectionString = scheduleRedisConn;
                    item.Update();
                }
            }
        }

        /// <summary>
        /// 更新消息推送的队列Redis服务器连接
        /// </summary>
        /// <param name="pushRedisConn"></param>
        public static void UpdatePushRedis(string pushRedisConn)
        {
            if (null != PushRedisConfigs)
            {
                foreach (var item in PushRedisConfigs.Items)
                {
                    item.ConnectionString = pushRedisConn;
                    item.Update();
                }
            }
        }

        /// <summary>
        /// 更新数据缓存的Redis服务器连接
        /// </summary>
        /// <param name="cacheRedisConn"></param>
        /// <param name="sqlType"></param>
        /// <param name="sqlConn"></param>
        public static void UpdateDataCacheRedis(string cacheRedisConn)
        {
            DataCacheRedisConnectionString = cacheRedisConn;
            InjectionDataCache();
        }

        /// <summary>
        /// 注入数据缓存
        /// </summary>
        public static void InjectionDataCache()
        {
            //注入数据缓存组件
            DataCacheInjection.UseDataCache(DataCacheRedisConnectionString, SqlType, KylinDBConnectionString);
        }

        /// <summary>
        /// 更新数据库连接
        /// </summary>
        /// <param name="sqlType"></param>
        /// <param name="connectionString"></param>
        public static void UpdateSqlConnection(SqlProviderType sqlType, string connectionString)
        {
            KylinDBConnectionString = connectionString;

            SqlType = sqlType;

            InjectionDataCache();
        }

        /// <summary>
        /// 更新队列服务配置
        /// </summary>
        public static void UpdateQueueConfig()
        {
            var sysConfigs = CacheCollection.SystemGolbalConfigCache.Value();

            #region //社区配置
            UpdateCircleConfig(sysConfigs);
            #endregion

            #region //福利配置
            UpdateWelfareConfig(sysConfigs);
            #endregion

            #region //上门预约订单自动服务参数配置
            UpdateAppointConfig(sysConfigs);
            #endregion

            #region //B2C商城订单自动服务参数配置
            UpdateB2COrderConfig(sysConfigs);
            #endregion

            #region //商家商品订单自动服务参数配置
            UpdateMerchantOrderConfig(sysConfigs);
            #endregion
        }

        /// <summary>
        /// 更新缓存维护参数配置
        /// </summary>
        public static void UpdateCacheMaintainConfig(IEnumerable<CacheMaintainConfig> configs = null)
        {
            var _list = new List<CacheMaintainConfig>();

            //缓存级别列表
            var levelList = typeof(CacheLevel).GetEnumDesc<CacheLevel>();

            foreach (var item in levelList)
            {
                var level = (CacheLevel)Enum.Parse(typeof(CacheLevel), item.Name);

                CacheMaintainConfig conf = new CacheMaintainConfig
                {
                    Level = level,
                    PeriodTime = 0,
                    TimeOption = default(CacheTimeOption)
                };


                CacheMaintainConfig _temp = null;
                if (null != configs)
                {
                    _temp = configs.FirstOrDefault(p => (int)p.Level == item.Value);
                }

                if (null != _temp)
                {
                    conf.PeriodTime = _temp.PeriodTime;
                    conf.TimeOption = _temp.TimeOption;
                }
                else
                {
                    switch (level)
                    {
                        case CacheLevel.Hight: conf.PeriodTime = 7; conf.TimeOption = CacheTimeOption.Day; break;
                        case CacheLevel.Lower: conf.PeriodTime = 30; conf.TimeOption = CacheTimeOption.Minute; break;
                        case CacheLevel.Middel: conf.PeriodTime = 1; conf.TimeOption = CacheTimeOption.Day; break;
                        case CacheLevel.Permanent: conf.PeriodTime = 180; conf.TimeOption = CacheTimeOption.Day; break;
                    }
                }

                if (conf.PeriodTime < 0) conf.PeriodTime = 0;


                _list.Add(conf);
            }

            CacheMaintainConfigs = _list;
        }

        #region 私有方法

        /// <summary>
        /// 上门预约订单自动服务参数配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void UpdateAppointConfig(List<SystemGolbalConfigCacheModel> values = null)
        {
            values = values ?? CacheCollection.SystemGolbalConfigCache.Value();

            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitUserDone);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitUserEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            AppointConfig = new AppointLateConfig
            {
                EndServiceWaitUserDays = waitDoneMinutes / (24 * 60),
                PaymentWaitMinutes = waitPayMinutes,
                EvaluateWaitDyas = waitEvalMimutes / (24 * 60)
            };
        }

        /// <summary>
        /// B2C商城订单自动服务参数配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void UpdateB2COrderConfig(List<SystemGolbalConfigCacheModel> values = null)
        {
            values = values ?? CacheCollection.SystemGolbalConfigCache.Value();

            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.B2COrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.B2COrderWaitReceive);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.B2COrderEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            B2COrderConfig = new B2COrderLateConfig
            {
                WaitPaymentMinutes = waitPayMinutes,
                WaitReceiptGoodsDays = waitDoneMinutes / (24 * 60),
                WaitEvaluateDays = waitEvalMimutes / (24 * 60)
            };
        }

        /// <summary>
        /// 商家订单自动服务参数配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void UpdateMerchantOrderConfig(List<SystemGolbalConfigCacheModel> values = null)
        {
            values = values ?? CacheCollection.SystemGolbalConfigCache.Value();

            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.MerchantOrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.MerchantOrderWaitReceive);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.MerchantOrderWaitEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            MerchantOrderConfig = new MerchantOrderLateConfig
            {
                WaitPaymentMinutes = waitPayMinutes,
                WaitReceiptGoodsDays = waitDoneMinutes / (24 * 60),
                WaitEvaluateDays = waitEvalMimutes / (24 * 60)
            };
        }

        /// <summary>
        /// 社区相关配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void UpdateCircleConfig(List<SystemGolbalConfigCacheModel> values = null)
        {
            values = values ?? CacheCollection.SystemGolbalConfigCache.Value();

            var remindTime = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.CircleEventRemind);

            CircleConfig = new CircleConfig
            {
                BeforeRemindMinutes = GetMinutes(remindTime.Value, remindTime.ValueUnit)
            };
        }

        /// <summary>
        /// 福利相关配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void UpdateWelfareConfig(List<SystemGolbalConfigCacheModel> values = null)
        {
            values = values ?? CacheCollection.SystemGolbalConfigCache.Value();

            var remindTime = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.WelfareApplyRemind);

            WelfareConfig = new WelfareConfig
            {
                BeforeRemindMinutes = GetMinutes(remindTime.Value, remindTime.ValueUnit)
            };
        }

        /// <summary>
        /// 获取分钟数
        /// </summary>
        /// <param name="val"></param>
        /// <param name="valUnit">对枚举<seealso cref="TimeOption"/>的值表示</param>
        /// <returns></returns>
        static int GetMinutes(string val, int valUnit)
        {
            int minutes = 0;
            int.TryParse(val, out minutes);

            if (valUnit == (int)TimeOption.Year) minutes *= 365 * 24 * 60;
            else if (valUnit == (int)TimeOption.Month) minutes *= 30 * 24 * 60;
            else if (valUnit == (int)TimeOption.Day) minutes *= 24 * 60;
            else if (valUnit == (int)TimeOption.Hour) minutes *= 60;
            else if (valUnit == (int)TimeOption.Second) minutes = minutes / 60;

            return minutes;
        }

        #endregion

        #region 成员
        /// <summary>
        /// 服务程序相关描述信息
        /// </summary>
        public static ProductInfo ProductInfo { get; private set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public static SqlProviderType SqlType { get; private set; }

        /// <summary>
        /// Kylin数据库连接字符串
        /// </summary>
        public static string KylinDBConnectionString { get; private set; }

        /// <summary>
        /// 数据缓存Redis连接字符串
        /// </summary>
        public static string DataCacheRedisConnectionString { get; private set; }

        /// <summary>
        /// 社区配置
        /// </summary>
        public static CircleConfig CircleConfig { get; private set; }

        /// <summary>
        /// 福利配置
        /// </summary>
        public static WelfareConfig WelfareConfig { get; private set; }

        /// <summary>
        /// 上门预约订单自动服务配置
        /// </summary>
        public static AppointLateConfig AppointConfig { get; private set; }

        /// <summary>
        /// B2C订单逾期自动服务配置
        /// </summary>
        public static B2COrderLateConfig B2COrderConfig { get; private set; }

        /// <summary>
        /// 商家订单逾期自动服务配置
        /// </summary>
        public static MerchantOrderLateConfig MerchantOrderConfig { get; private set; }

        /// <summary>
        /// 任务计划数据在Redis缓存中的配置
        /// </summary>
        public static ScheduleRedisCollection ScheduleRedisConfigs { get; private set; }

        /// <summary>
        /// 推送消息 Redis缓存配置 
        /// </summary>
        public static PushRedisCollection PushRedisConfigs { get; private set; }

        /// <summary>
        /// 缓存维护参数配置
        /// </summary>
        public static List<CacheMaintainConfig> CacheMaintainConfigs { get; private set; }

        #endregion
    }
}

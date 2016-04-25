using KylinService.Core;
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
            #region //Kylin数据库连接字符串

            KylinDBConnectionString = ConfigurationManager.ConnectionStrings["KylinConnectionString"].ConnectionString;

            #endregion

            #region  //注入

            string dataCacheRedisConn = ConfigurationManager.ConnectionStrings["RedisDataCacheConnectionString"].ConnectionString;
            //注入数据缓存组件
            DataCacheInjection.UseDataCache(dataCacheRedisConn, SqlProviderType.PostgreSQL, KylinDBConnectionString);

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
        /// 更新队列服务配置
        /// </summary>
        public static void UpdateQueueConfig()
        {
            var sysConfigs = CacheCollection.SystemGolbalConfigCache.Value();

            #region //社区配置
            CircleConfig = GetCircleConfig(sysConfigs);
            #endregion

            #region //福利配置
            WelfareConfig = GetWelfareConfig(sysConfigs);
            #endregion

            #region //上门预约订单自动服务参数配置
            AppointConfig = GetAppointConfig(sysConfigs);
            #endregion

            #region //B2C商城订单自动服务参数配置
            B2COrderConfig = GetB2COrderConfig(sysConfigs);
            #endregion

            #region //商家商品订单自动服务参数配置
            MerchantOrderConfig = GetMerchantOrderConfig(sysConfigs);
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
        static AppointLateConfig GetAppointConfig(List<SystemGolbalConfigCacheModel> values)
        {
            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitUserDone);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitUserEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            return new AppointLateConfig
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
        static B2COrderLateConfig GetB2COrderConfig(List<SystemGolbalConfigCacheModel> values)
        {
            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.B2COrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.B2COrderWaitReceive);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.B2COrderEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            return new B2COrderLateConfig
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
        static MerchantOrderLateConfig GetMerchantOrderConfig(List<SystemGolbalConfigCacheModel> values)
        {
            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.MerchantOrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.MerchantOrderWaitReceive);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.MerchantOrderWaitEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            return new MerchantOrderLateConfig
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
        static CircleConfig GetCircleConfig(List<SystemGolbalConfigCacheModel> values)
        {
            var remindTime = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.CircleEventRemind);

            return new CircleConfig
            {
                BeforeRemindMinutes = GetMinutes(remindTime.Value, remindTime.ValueUnit)
            };
        }

        /// <summary>
        /// 福利相关配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        static WelfareConfig GetWelfareConfig(List<SystemGolbalConfigCacheModel> values)
        {
            var remindTime = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.WelfareApplyRemind);

            return new WelfareConfig
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
        /// Kylin数据库连接字符串
        /// </summary>
        public static string KylinDBConnectionString;

        /// <summary>
        /// 社区配置
        /// </summary>
        public static CircleConfig CircleConfig;

        /// <summary>
        /// 福利配置
        /// </summary>
        public static WelfareConfig WelfareConfig;

        /// <summary>
        /// 上门预约订单自动服务配置
        /// </summary>
        public static AppointLateConfig AppointConfig;

        /// <summary>
        /// B2C订单逾期自动服务配置
        /// </summary>
        public static B2COrderLateConfig B2COrderConfig;

        /// <summary>
        /// 商家订单逾期自动服务配置
        /// </summary>
        public static MerchantOrderLateConfig MerchantOrderConfig;

        /// <summary>
        /// 任务计划数据在Redis缓存中的配置
        /// </summary>
        public static ScheduleRedisCollection ScheduleRedisConfigs;

        /// <summary>
        /// 推送消息 Redis缓存配置 
        /// </summary>
        public static PushRedisCollection PushRedisConfigs;

        /// <summary>
        /// 缓存维护参数配置
        /// </summary>
        public static List<CacheMaintainConfig> CacheMaintainConfigs;

        #endregion
    }
}

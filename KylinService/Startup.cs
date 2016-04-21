using KylinService.Redis;
using KylinService.Services.Appoint;
using KylinService.Services.MallOrderLate;
using KylinService.Services.MerchantOrderLate;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Td.Kylin.DataCache;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService
{
    internal sealed class Startup
    {
        public static void Init()
        {
            #region //Kylin数据库连接字符串

            KylinDBConnectionString = ConfigurationManager.ConnectionStrings["KylinConnectionString"].ConnectionString;

            #endregion

            #region //自助处理数据所在的Redis服务配置

            AutoDataRedisConfig = RedisConfigManager.Config;

            #endregion

            #region  //注入

            //注入Redis
            RedisInjection.UseRedis(AutoDataRedisConfig.ConnectString);

            string dataCacheRedisConn = ConfigurationManager.ConnectionStrings["RedisDataCacheConnectionString"].ConnectionString;
            //注入数据缓存组件
            DataCacheInjection.UseDataCache(dataCacheRedisConn, SqlProviderType.PostgreSQL, KylinDBConnectionString);

            #endregion            

            var sysConfigs = CacheCollection.SystemGolbalConfigCache.Value();

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
        /// 上门预约订单自动服务参数配置
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        static AppointConfig GetAppointConfig(List<SystemGolbalConfigCacheModel> values)
        {
            var waitPay = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitPayment);
            var waitDone = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitUserDone);
            var waitEval = values.FirstOrDefault(p => p.ResourceType == (int)GlobalConfigType.Time && p.ResourceKey == (int)GlobalTimeConfigOption.ServiceOrderWaitUserEvaluate);

            int waitPayMinutes = GetMinutes(waitPay.Value, waitPay.ValueUnit);
            int waitDoneMinutes = GetMinutes(waitDone.Value, waitDone.ValueUnit);
            int waitEvalMimutes = GetMinutes(waitEval.Value, waitEval.ValueUnit);

            return new AppointConfig
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

        /// <summary>
        /// Kylin数据库连接字符串
        /// </summary>
        public static string KylinDBConnectionString;

        /// <summary>
        /// 上门预约订单自动服务配置
        /// </summary>
        public static AppointConfig AppointConfig;

        /// <summary>
        /// B2C订单逾期自动服务配置
        /// </summary>
        public static B2COrderLateConfig B2COrderConfig;

        /// <summary>
        /// 商家订单逾期自动服务配置
        /// </summary>
        public static MerchantOrderLateConfig MerchantOrderConfig;

        /// <summary>
        /// 自助处理数据所在的Redis服务配置
        /// </summary>
        public static RedisConfig AutoDataRedisConfig;
    }
}

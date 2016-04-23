using StackExchange.Redis;
using System;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    internal sealed class DataCacheMiddleware
    {

        /// <summary>
        /// The options relevant to a set of redis connections
        /// </summary>
        private readonly ConfigurationOptions _options;

        /// <summary>
        /// 数据库类型
        /// </summary>
        private readonly SqlProviderType _sqlProviderType;

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly string _sqlconnectionString;

        

        #region 非Web程序中使用

        /// <summary>
        /// 实例化（非Web程序中使用）
        /// </summary>
        /// <param name="redisOptions"></param>
        /// <param name="sqlType">数据库类型</param>
        /// <param name="sqlConnection">数据库连接字符串</param>
        public DataCacheMiddleware(ConfigurationOptions redisOptions, SqlProviderType sqlType, string sqlConnection)
        {
            if (redisOptions == null)
            {
                throw new ArgumentNullException(nameof(redisOptions));
            }
            if (string.IsNullOrWhiteSpace(sqlConnection))
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }
            _sqlProviderType = sqlType;
            _sqlconnectionString = sqlConnection;
            _options = redisOptions;
        }

        /// <summary>
        /// 实例化（非Web程序中使用）
        /// </summary>
        /// <param name="next"></param>
        /// <param name="redisConnection">Redis 连接字符串</param>
        /// <param name="sqlType">数据库类型</param>
        /// <param name="sqlConnection">数据库连接字符串</param>
        public DataCacheMiddleware(string redisConnection, SqlProviderType sqlType, string sqlConnection)
        {
            var tempOptions = ConfigurationOptions.Parse(redisConnection);

            if (tempOptions == null)
            {
                throw new ArgumentNullException(nameof(redisConnection));
            }
            if (string.IsNullOrWhiteSpace(sqlConnection))
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }
            _sqlProviderType = sqlType;
            _sqlconnectionString = sqlConnection;
            _options = tempOptions;
        }

        public void Invoke()
        {
            RedisInjection.UseRedis(_options);

            CacheStartup.SqlType = _sqlProviderType;

            CacheStartup.SqlConnctionString = _sqlconnectionString;
        }

        #endregion
    }
}

﻿using KylinService.SysEnums;
using StackExchange.Redis;
using Td.Kylin.Redis;

namespace KylinService.Redis.Push
{
    /// <summary>
    /// Redis存储Key及数据库序号
    /// </summary>
    public class PushKeysConfig
    {
        /// <summary>
        /// 存储类型
        /// </summary>
        public RedisPushType SaveType { get; set; }

        /// <summary>
        /// 存储的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 数据库序号
        /// </summary>
        public int DbIndex { get; set; }

        /// <summary>
        /// Redis服务器连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        private IDatabase _database;
        /// <summary>
        /// 数据库
        /// </summary>
        public IDatabase DataBase
        {
            get
            {
                if (null == _database || !_database.Multiplexer.IsConnected)
                {
                    var options = ConfigurationOptions.Parse(ConnectionString);

                    _database = ConnectionMultiplexer.Connect(options).GetDatabase(DbIndex);
                }
                return _database;
            }
            set
            {
                _database = value;
            }
        }
    }
}

using KylinService.Services;
using KylinService.SysEnums;
using StackExchange.Redis;
using System;
using Td.Kylin.Redis;

namespace KylinService.Redis.Schedule
{
    /// <summary>
    /// 任务计划队列Redis配置
    /// </summary>
    public class ScheduleRedisConfig : ServiceState
    {
        /// <summary>
        /// 任务计划名称
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// 对应的任务类型数据在Redis中存储的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 存储所在的Redis数据库index
        /// </summary>
        public int DbIndex { get; set; }

        /// <summary>
        /// 备份区数据库index
        /// </summary>
        public int BackupDBindex { get; set; }

        /// <summary>
        /// Redis连接
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 计划任务类型
        /// </summary>
        public QueueScheduleType Type
        {
            get
            {
                return (QueueScheduleType)Enum.Parse(typeof(QueueScheduleType), ScheduleName);
            }
        }

        /// <summary>
        /// 备份区Database
        /// </summary>
        public IDatabase BackupDB
        {
            get
            {
                return GetDB(BackupDBindex);
            }
        }

        /// <summary>
        /// RedisContext
        /// </summary>
        public RedisContext RedisContext { get; set; }

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
                    _database = GetDB(DbIndex);
                }

                return _database;
            }
        }

        private IDatabase GetDB(int dbIndex)
        {
            if (null == RedisContext || !RedisContext.IsConnected)
            {
                RedisContext = new RedisContext(ConnectionString, true);
            }

            return RedisContext.GetDatabase(dbIndex);
        }
    }
}

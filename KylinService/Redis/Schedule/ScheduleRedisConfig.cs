using KylinService.SysEnums;
using StackExchange.Redis;
using System;

namespace KylinService.Redis.Schedule
{
    /// <summary>
    /// 任务计划队列Redis配置
    /// </summary>
    public class ScheduleRedisConfig
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

        private IDatabase _database;
        /// <summary>
        /// 数据库
        /// </summary>
        public IDatabase DataBase
        {
            get
            {
                if (null == _database)
                {
                    var options = ConfigurationOptions.Parse(ConnectionString);
                    _database = ConnectionMultiplexer.Connect(options).GetDatabase(DbIndex);
                }
                return _database;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            _database = null;
        }
    }
}

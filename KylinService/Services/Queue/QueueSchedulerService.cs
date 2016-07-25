using KylinService.Core;
using KylinService.Redis.Schedule;
using KylinService.SysEnums;
using StackExchange.Redis;
using System;
using System.Threading;
using Td.Kylin.Redis;

namespace KylinService.Services.Queue
{
    /// <summary>
    /// 队列服务抽象类
    /// </summary>
    public abstract class QueueSchedulerService<T> : SchedulerService where T : ServiceState
    {
        private volatile object _locker = new object();

        /// <summary>
        /// 是否循环处理
        /// </summary>
        private bool _isLoop;

        /// <summary>
        /// 循环队列为空时默认等待下次执行的时间（毫秒）间隔，如连续为empty，等待时间为当前设置时间的倍数
        /// </summary>
        private int _defaultNextWaitMillisecondsIfEmpty;

        /// <summary>
        /// 队列为空时阶梯等待时间的最大倍数
        /// </summary>
        private const int _maxWaitTimes = 20;

        /// <summary>
        /// 当前已等待时间倍数
        /// </summary>
        private int _waitTimes = 0;

        /// <summary>
        /// 服务类型
        /// </summary>
        public QueueScheduleType ServiceType;

        /// <summary>
        /// 任务计划数据所在Redis配置
        /// </summary>
        protected ScheduleRedisConfig RedisConfig;

        private ConnectionMultiplexer _redis;

        /// <summary>
        /// QueueRedis（ConnectionMultiplexer）
        /// </summary>
        protected ConnectionMultiplexer QueueRedis
        {
            get
            {
                if (_redis == null || !_redis.IsConnected)
                {
                    lock (_locker)
                    {
                        if (_redis == null || !_redis.IsConnected)
                        {
                            _redis = ConnectionMultiplexer.Connect(RedisConfig.ConnectionString);
                        }
                    }
                }

                return _redis;
            }
        }

        /// <summary>
        /// 请求队列Database
        /// </summary>
        protected IDatabase QuequDatabase
        {
            get
            {
                return GetRedisDatabase(RedisConfig.DbIndex);
            }
        }

        /// <summary>
        /// 备份区Database
        /// </summary>
        protected IDatabase BackupDatabase
        {
            get
            {
                return GetRedisDatabase(RedisConfig.BackupDBindex);
            }
        }

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="scheduleType"></param>
        /// <param name="isLoop">是否循环处理</param>
        /// <param name="defaultNextWaitMillisecondsIfEmpty">循环队列为空时默认等待下次执行的时间（毫秒）间隔，如连续为empty，等待时间为当前设置时间的倍数</param>
        public QueueSchedulerService(QueueScheduleType scheduleType, bool isLoop = true, int defaultNextWaitMillisecondsIfEmpty = 100)
        {

            ServiceType = scheduleType;

            ServiceName = SysData.GetQueueServiceName((int)ServiceType);

            RedisConfig = Startup.ScheduleRedisConfigs[scheduleType];

            this._isLoop = isLoop;

            this._defaultNextWaitMillisecondsIfEmpty = defaultNextWaitMillisecondsIfEmpty;
        }

        /// <summary>
        /// 服务启动/开始
        /// </summary>
        public override void Start()
        {
            this.IsPaused = false;

            ThreadPool.QueueUserWorkItem((item) =>
            {
                StartFirstRequest();

                while (true)
                {
                    bool onContinue = false;

                    //未暂停时
                    if (!IsPaused)
                    {
                        //执行单次请求并返回是否需要继续信号指示
                        onContinue = SingleMain();
                    }

                    //不继续时
                    if (!onContinue)
                    {
                        if (_waitTimes < _maxWaitTimes)
                        {
                            _waitTimes++;
                        }

                        //休眠时间（单位：毫秒）
                        var waitMilliseconds = _defaultNextWaitMillisecondsIfEmpty * _waitTimes;

                        //休眠一段时间（单位：毫秒），避免CPU空转
                        Thread.Sleep(waitMilliseconds);
                    }
                    else
                    {
                        _waitTimes = 0;
                    }
                }
            });
        }

        /// <summary>
        /// 单个实体对象task处理器
        /// </summary>
        /// <param name="model"><seealso cref="T"/>数据实体对象</param>
        /// <param name="mustBackup">是否需要备份数据到备份区</param>
        /// <returns></returns>
        protected abstract bool EntityTaskHandler(T model, bool mustBackup = true);

        /// <summary>
        /// 服务启动后首个请求执行
        /// </summary>
        protected virtual void StartFirstRequest()
        {
            try
            {
                if (null != BackupDatabase)
                {
                    var backDataDic = BackupDatabase.HashGetAll<T>(RedisConfig.Key);

                    if (null == backDataDic || backDataDic.Count < 1) return;

                    var backDataList = backDataDic.Values;

                    foreach (T item in backDataList)
                    {
                        EntityTaskHandler(item, false);
                    }
                }
            }
            catch (Exception ex)
            {
                OnThrowException(ex);
            }
        }

        /// <summary>
        /// 业务完成前备份数据
        /// </summary>
        /// <param name="data">数据对象</param>
        protected virtual void BackBeforeDone(RedisValue hashField, T data)
        {
            if (null == BackupDatabase)
            {
                WriteMessageHelper.WriteMessage("Redis(database)在将数据备份到备份区时连接丢失，source:" + this.ServiceName + "，Method:" + this.Me());
            }
            else
            {
                BackupDatabase.HashSetAsync(RedisConfig.Key, hashField, data);
            }
        }

        /// <summary>
        /// 业务完成后删除备份数据
        /// </summary>
        /// <param name="field">备份的主字段</param>
        protected virtual void DeleteBackAfterDone(RedisValue field)
        {
            if (string.IsNullOrWhiteSpace(field)) return;

            if (null == BackupDatabase)
            {
                WriteMessageHelper.WriteMessage("Redis(database)在将数据从备份区中删除时连接丢失，source:" + this.ServiceName + "，Method:" + this.Me());
            }
            else
            {
                BackupDatabase.HashDeleteAsync(RedisConfig.Key, field);
            }
        }

        /// <summary>
        /// 获取Redis的数据库 select dbindex
        /// </summary>
        /// <param name="dbindex"></param>
        /// <returns></returns>
        private IDatabase GetRedisDatabase(int dbindex)
        {
            if (null != QueueRedis)
            {
                return QueueRedis.GetDatabase(dbindex);
            }
            else
            {
                return null;
            }
        }
    }
}

using StackExchange.Redis;

namespace KylinService.Redis.Push
{
    public class PushRedisContext
    {
        /// <summary>
        /// 锁对象
        /// </summary>
        public static object _locker = new object();

        private static ConnectionMultiplexer _redis;

        /// <summary>
        /// Redis（ConnectionMultiplexer）对象，单例
        /// </summary>
        public static ConnectionMultiplexer Redis
        {
            get
            {
                if (_redis == null)
                {
                    lock (_redis)
                    {
                        if (_redis == null)
                        {
                            _redis = ConnectionMultiplexer.Connect(PushRedisConfigManager.RedisConnectionString);
                        }
                    }
                }

                return _redis;
            }
        }
    }
}

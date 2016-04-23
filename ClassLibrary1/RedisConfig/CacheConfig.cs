namespace Td.Kylin.DataCache
{
    /// <summary>
    /// 缓存项配置
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// 缓存项类型
        /// </summary>
        public CacheItemType ItemType { get; set; }

        /// <summary>
        /// Redis存储Key
        /// </summary>
        public string RedisKey { get; set; }

        /// <summary>
        /// Redis的数据库序号
        /// </summary>
        public int RedisDbIndex { get; set; }

        /// <summary>
        /// Redis中存储类型
        /// </summary>
        public RedisSaveType SaveType { get; set; }

        /// <summary>
        /// 缓存级别
        /// </summary>
        public CacheLevel Level { get; set; }
    }
}

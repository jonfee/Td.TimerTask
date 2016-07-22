using KylinService.SysEnums;
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
    }
}

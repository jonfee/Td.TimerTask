using KylinService.SysEnums;
using System.Collections.Generic;

namespace KylinService.Redis
{
    /// <summary>
    /// Redis配置信息
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// Redis服务器连接配置信息
        /// </summary>
        public string ConnectString { get; set; }

        /// <summary>
        /// 数据存储Key与所在数据库序号
        /// </summary>
        public List<KeysConfig> DbKeys { get; set; }
    }
}

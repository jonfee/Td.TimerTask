using KylinService.SysEnums;

namespace KylinService.Redis
{
    /// <summary>
    /// Redis存储Key及数据库序号
    /// </summary>
    public class KeysConfig
    {
        /// <summary>
        /// 存储类型
        /// </summary>
        public RedisSaveType SaveType { get; set; }

        /// <summary>
        /// 存储的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 数据库序号
        /// </summary>
        public int DBindex { get; set; }
    }
}

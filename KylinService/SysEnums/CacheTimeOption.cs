using System.ComponentModel;

namespace KylinService.SysEnums
{
    /// <summary>
    /// 缓存更新周期单位
    /// </summary>
    public enum CacheTimeOption
    {
        /// <summary>
        /// 天
        /// </summary>
        [Description("天")]
        Day,
        /// <summary>
        /// 小时
        /// </summary>
        [Description("小时")]
        Hour,
        /// <summary>
        /// 分钟
        /// </summary>
        [Description("分钟")]
        Minute
    }
}

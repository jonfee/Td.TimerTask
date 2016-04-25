using KylinService.SysEnums;
using Td.Kylin.DataCache;

namespace KylinService.Services.CacheMaintain
{
    /// <summary>
    /// 缓存维护计划配置项
    /// </summary>
    public class CacheMaintainConfig
    {
        public CacheMaintainConfig() { }

        public CacheMaintainConfig(CacheMaintainConfig config)
        {
            if (null != config)
            {
                Level = config.Level;
                PeriodTime = config.PeriodTime;
                TimeOption = config.TimeOption;
            }
        }

        /// <summary>
        /// 缓存级别
        /// </summary>
        public CacheLevel Level { get; set; }

        /// <summary>
        /// 缓存周期
        /// </summary>
        public int PeriodTime { get; set; }

        /// <summary>
        /// 缓存周期单位
        /// </summary>
        public CacheTimeOption TimeOption { get; set; }
    }
}

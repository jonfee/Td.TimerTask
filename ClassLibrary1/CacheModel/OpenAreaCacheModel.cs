using System;

namespace Td.Kylin.DataCache.CacheModel
{
    /// <summary>
    /// 已开通区域缓存模型
    /// </summary>
    public class OpenAreaCacheModel
    {
        /// <summary>
        /// HashField（AreaID）
        /// </summary>
        public string HashField
        {
            get
            {
                return AreaID.ToString();
            }
        }

        /// <summary>
        /// 区域ID
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 地区名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public bool Status { get; set; }
    }
}

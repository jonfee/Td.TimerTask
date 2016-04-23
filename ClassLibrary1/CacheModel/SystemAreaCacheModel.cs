namespace Td.Kylin.DataCache.CacheModel
{
    /// <summary>
    /// 全国区域缓存模型
    /// </summary>
    public class SystemAreaCacheModel
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
        /// 是否下级
        /// </summary>
        public bool HasChild { get; set; }
        /// <summary>
        /// 区域路径（如：110000|110100|110101）
        /// </summary>
        public string Layer { get; set; }

    }
}

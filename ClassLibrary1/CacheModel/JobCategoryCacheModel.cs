namespace Td.Kylin.DataCache.CacheModel
{
    /// <summary>
    /// 职位类别缓存模型
    /// </summary>
    public class JobCategoryCacheModel
    {
        /// <summary>
        /// HashField（CategoryID）
        /// </summary>
        public string HashField
        {
            get
            {
                return CategoryID.ToString();
            }
        }

        /// <summary>
        /// 岗位分类ID
        /// </summary>
        public long CategoryID { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父ID
        /// </summary>
        public long ParentID { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public long OrderNo { get; set; }

        /// <summary>
        /// 标识状态集（热门）
        /// </summary>
        public int TagStatus { get; set; }
    }
}

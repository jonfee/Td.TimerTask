namespace Td.Kylin.DataCache.CacheModel
{
    /// <summary>
    /// 商家商品系统分类缓存模型
    /// </summary>
    public class MerchantProductSystemCategoryCacheModel
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
        /// 分类ID
        /// </summary>
        public long CategoryID { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父级分类ID（顶级分类时为0）
        /// </summary>
        public long ParentCategoryID { get; set; }

        /// <summary>
        /// 分类层级路径（如：1|2|3）
        /// </summary>
        public string CategoryPath { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderNo { get; set; }
    }
}

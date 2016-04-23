namespace Td.Kylin.DataCache.CacheModel
{
    public class B2CProductCategoryTagCacheModel
    {
        /// <summary>
        /// HashField（TagID）
        /// </summary>
        public string HashField
        {
            get
            {
                return TagID.ToString();
            }
        }

        ///<summary>
        ///标签ID
        ///</summary>
        public long TagID { get; set; }

        ///<summary>
        ///商品类目ID
        ///</summary>
        public long CategoryID { get; set; }

        ///<summary>
        ///商品标签名称
        ///</summary>
        public string TagName { get; set; }

        ///<summary>
        ///排序值
        ///</summary>
        public int OrderNo { get; set; }
    }
}

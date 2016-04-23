namespace Td.Kylin.DataCache.CacheModel
{
    public class AreaForumCacheModel
    {
        /// <summary>
        /// HashField（AreaForumID）
        /// </summary>
        public string HashField
        {
            get
            {
                return AreaForumID.ToString();
            }
        }

        ///<summary>
		/// 区域圈子ID
		///</summary>
		public long AreaForumID { get; set; }

        /// <summary>
        /// 所属区域ID
        /// </summary>
        public int AreaID { get; set; }

        ///<summary>
        /// 版面圈子ID
        ///</summary>
        public long ForumID { get; set; }

        /// <summary>
        /// 圈子别名
        /// </summary>
        public string AliasName { get; set; }

        ///<summary>
        ///论坛分类ID
        ///</summary>
        public long CategoryID { get; set; }

        ///<summary>
        ///论坛图标
        ///</summary>
        public string Logo { get; set; }

        ///<summary>
        ///论坛介绍
        ///</summary>
        public string Description { get; set; }

        ///<summary>
        ///版主
        ///</summary>
        public string Moderators { get; set; }

        ///<summary>
        ///论坛分类排序
        ///</summary>
        public int OrderNo { get; set; }

        ///<summary>
        ///发帖类型2n次方
        ///</summary>
        public int PostType { get; set; }

        ///<summary>
        ///发帖等级限制
        ///</summary>
        public int PostLevel { get; set; }

        ///<summary>
        ///不需要审核等级限制
        ///</summary>
        public int PassLevel { get; set; }
    }
}

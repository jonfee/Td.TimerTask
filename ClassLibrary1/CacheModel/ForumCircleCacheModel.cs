namespace Td.Kylin.DataCache.CacheModel
{
    public class ForumCircleCacheModel
    {
        /// <summary>
        /// HashField（ForumID）
        /// </summary>
        public string HashField
        {
            get
            {
                return ForumID.ToString();
            }
        }

        ///<summary>
        ///版面圈子ID
        ///</summary>
        public long ForumID { get; set; }

        ///<summary>
        ///论坛分类ID
        ///</summary>
        public long CategoryID { get; set; }

        ///<summary>
        ///论坛名称
        ///</summary>
        public string ForumName { get; set; }

        ///<summary>
        ///论坛图标
        ///</summary>
        public string Logo { get; set; }

        ///<summary>
        ///论坛介绍
        ///</summary>
        public string Description { get; set; }

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

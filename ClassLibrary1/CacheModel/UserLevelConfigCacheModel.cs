namespace Td.Kylin.DataCache.CacheModel
{
    public class UserLevelConfigCacheModel
    {
        /// <summary>
        /// HashField（LevelID）
        /// </summary>
        public string HashField
        {
            get
            {
                return LevelID.ToString();
            }
        }

        ///<summary>
        ///数据ID
        ///</summary>
        public long LevelID { get; set; }

        ///<summary>
        ///等级名称
        ///</summary>
        public string Name { get; set; }

        ///<summary>
        ///最小经验值
        ///</summary>
        public int Min { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
    }
}

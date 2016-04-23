namespace Td.Kylin.DataCache.CacheModel
{
    public class SystemGolbalConfigCacheModel
    {
        /// <summary>
        /// HashField（由“ResourceType_ResourceKey”组成）
        /// </summary>
        public string HashField
        {
            get
            {
                return string.Format("{0}{1}", ResourceType, ResourceKey);
            }
        }

        /// <summary>
        /// 资源类型（如：摇一摇配置｜时间配置｜消息模板配置｜短信模板配置｜默认区域抽成配置）
        /// </summary>
        public int ResourceType { get; set; }

        /// <summary>
        /// 资源类型键（如：每日摇一摇次数｜B2C订单抽成｜注册时手机验证码）
        /// </summary>
        public int ResourceKey { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 资源值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 值单位描述（例如：时间配置时1表示年，抽成配置时1表示按金额百分比）
        /// </summary>
        public int ValueUnit { get; set; }
    }
}

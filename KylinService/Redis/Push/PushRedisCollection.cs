using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KylinService.Redis.Push
{
    /// <summary>
    /// 推送消息Redis配置信息
    /// </summary>
    public class PushRedisCollection
    {
        /// <summary>
        /// 数据存储Key与所在数据库序号
        /// </summary>
        public List<PushKeysConfig> Items { get; set; }

        public PushKeysConfig this[string key]
        {
            get
            {
                if (Contains(key))
                {
                    return Items.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                }
                return null;
            }
            set
            {
                if (Contains(key))
                {
                    var item = Items.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                    Items.Remove(item);
                    Items.Add(value);
                }
                else
                {
                    if (null == Items) Items = new List<PushKeysConfig>();
                    Items.Add(value);
                }
            }
        }

        public PushKeysConfig this[RedisPushType type]
        {
            get
            {
                if (Contains(type))
                {
                    return Items.FirstOrDefault(p => p.SaveType.Equals(type));
                }
                return null;
            }
            set
            {
                if (Contains(type))
                {
                    var item = Items.FirstOrDefault(p => p.SaveType.Equals(type));
                    Items.Remove(item);
                    Items.Add(value);
                }
                else
                {
                    if (null == Items) Items = new List<PushKeysConfig>();
                    Items.Add(value);
                }
            }
        }

        public bool Contains(string key)
        {
            if (null != Items)
            {
                return Items.Count(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) > 0;
            }

            return false;
        }

        public bool Contains(RedisPushType type)
        {
            if (null != Items)
            {
                return Items.Count(p => p.SaveType.Equals(type)) > 0;
            }

            return false;
        }
    }
}

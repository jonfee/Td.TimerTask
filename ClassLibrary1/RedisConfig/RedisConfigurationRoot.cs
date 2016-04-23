using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Td.Kylin.DataCache.RedisConfig
{
    /// <summary>
    /// 缓存配置
    /// </summary>
    public sealed class RedisConfigurationRoot
    {
        private List<CacheConfig> _collections;

        private readonly static object mylock = new object();

        /// <summary>
        /// 缓存配置项集合
        /// </summary>
        public List<CacheConfig> Collections
        {
            get
            {
                return _collections;
            }
            set
            {
                _collections = value;
            }
        }

        /// <summary>
        /// 获取/设置Redis缓存配置（Redis中的Key为对应itemType的枚举名称）
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="redisDbIndex"></param>
        /// <param name="saveType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public void Add(CacheItemType itemType, int redisDbIndex, RedisSaveType saveType, CacheLevel level)
        {
            var config = new CacheConfig
            {
                ItemType = itemType,
                RedisDbIndex = redisDbIndex,
                RedisKey = itemType.ToString(),
                SaveType = saveType,
                Level = level
            };

            lock (mylock)
            {
                if (null == _collections) _collections = new List<CacheConfig>();

                if (!Types.Contains(itemType))
                {
                    _collections.Add(config);
                }
                else
                {
                    var item = _collections.FirstOrDefault(p => p.ItemType == itemType);

                    _collections.Remove(item);

                    _collections.Add(config);
                }
            }
        }

        /// <summary>
        /// 获取/设置Redis缓存配置
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="redisKey"></param>
        /// <param name="redisDbIndex"></param>
        /// <param name="saveType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public void Add(CacheItemType itemType, string redisKey, int redisDbIndex, RedisSaveType saveType, CacheLevel level)
        {
            var config = new CacheConfig
            {
                ItemType = itemType,
                RedisDbIndex = redisDbIndex,
                RedisKey = redisKey,
                SaveType = saveType,
                Level = level
            };

            lock (mylock)
            {
                if (null == _collections) _collections = new List<CacheConfig>();

                if (!Types.Contains(itemType))
                {
                    _collections.Add(config);
                }
                else
                {
                    var item = _collections.FirstOrDefault(p => p.ItemType == itemType);

                    _collections.Remove(item);

                    _collections.Add(config);
                }
            }
        }

        /// <summary>
        /// 缓存项类型集合
        /// </summary>
        public CacheItemType[] Types
        {
            get
            {
                if (null != Collections)
                {
                    return Collections.Select(p => p.ItemType).ToArray();
                }

                return null;
            }
        }

        /// <summary>
        /// 缓存项Key集合
        /// </summary>
        public string[] Keys
        {
            get
            {
                if (null != Collections)
                {
                    return Collections.Select(p => p.RedisKey).ToArray();
                }

                return null;
            }
        }

        #region 索引器

        /// <summary>
        /// 定义索引器
        /// </summary>
        /// <param name="type">缓存项类型</param>
        /// <returns></returns>
        public CacheConfig this[CacheItemType type]
        {
            get
            {
                if (null != Collections)
                {
                    return Collections.FirstOrDefault(p => p.ItemType == type);
                }

                return null;
            }
            set
            {
                if (null == _collections) _collections = new List<CacheConfig>();

                if (!Types.Contains(type))
                {
                    _collections.Add(value);
                }
                else
                {
                    var item = _collections.FirstOrDefault(p => p.ItemType == type);

                    _collections.Remove(item);

                    _collections.Add(value);
                }
            }
        }

        /// <summary>
        /// 定义索引器
        /// </summary>
        /// <param name="redisKey">Redis缓存中的Key</param>
        /// <returns></returns>
        public CacheConfig this[string redisKey]
        {
            get
            {
                if (null != Collections)
                {
                    return Collections.FirstOrDefault(p => p.RedisKey.Equals(redisKey, StringComparison.OrdinalIgnoreCase));
                }

                return null;
            }
            set
            {
                if (null == _collections) _collections = new List<CacheConfig>();

                if (!Keys.Contains(redisKey))
                {
                    _collections.Add(value);
                }
                else
                {
                    var item = _collections.FirstOrDefault(p => p.RedisKey == redisKey);

                    _collections.Remove(item);

                    _collections.Add(value);
                }
            }
        }

        #endregion
    }
}

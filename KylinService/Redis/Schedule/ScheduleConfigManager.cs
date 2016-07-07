using KylinService.Core.Loger;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using Td.Kylin.Redis;

namespace KylinService.Redis.Schedule
{
    public sealed class ScheduleConfigManager : IConfigurationSectionHandler
    {
        /// <summary>
        /// Redis配置项
        /// </summary>
        public static ScheduleRedisCollection Collection { get; private set; }

        static ScheduleConfigManager()
        {
            ConfigurationManager.GetSection("scheduleRedisConnection");
        }

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            var _collection = new ScheduleRedisCollection();

            ScheduleRedisConfig defaultConfig = null;

            RedisContext defaultRedisContext = null;

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var name = node.Name;

                    if (name.ToLower() == "add")
                    {
                        var config = GetConfig(node);

                        if (config.ScheduleName.ToLower() == "default")
                        {
                            defaultConfig = config;

                            var options = ConfigurationOptions.Parse(defaultConfig.ConnectionString);

                            defaultRedisContext = new RedisContext(options);
                        }
                        else
                        {
                            if (config == null) continue;
                            if (string.IsNullOrWhiteSpace(config.Key)) continue;
                            if (defaultConfig != null && string.IsNullOrWhiteSpace(config.ConnectionString))
                            {
                                config.ConnectionString = defaultConfig.ConnectionString;
                            }
                            if (defaultConfig != null && config.DbIndex < 0)
                            {
                                config.DbIndex = defaultConfig.DbIndex;
                            }

                            _collection[config.ScheduleName] = config;
                        }
                    }
                }
            }

            _collection.Items.ForEach((item) =>
            {
                //如果当前redis连接跟默认连接一致，则初始化Database
                if (defaultConfig.ConnectionString == item.ConnectionString)
                {
                    item.RedisContext = defaultRedisContext;
                }
                else
                {
                    item.RedisContext = new RedisContext(item.ConnectionString);
                }
            });

            Collection = _collection;

            return Collection;
        }

        private static ScheduleRedisConfig GetConfig(XmlNode node)
        {
            var name = GetAttributeValue(node, "name");

            var key = GetAttributeValue(node, "redisKey");

            int dbIndex = 0;
            if (!int.TryParse(GetAttributeValue(node, "databaseIndex"), out dbIndex))
            {
                dbIndex = -1;
            }

            string connString = GetAttributeValue(node, "connectionString");

            return new ScheduleRedisConfig
            {
                ScheduleName = name,
                Key = key,
                DbIndex = dbIndex,
                ConnectionString = connString
            };
        }

        /// <summary>
        /// 获取配置节点上的属性值
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetAttributeValue(XmlNode node, string name)
        {
            return null != node.Attributes[name] ? node.Attributes[name].Value : string.Empty;
        }
    }
}

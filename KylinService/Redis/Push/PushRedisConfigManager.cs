using KylinService.Core.Loger;
using KylinService.SysEnums;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using Td.Kylin.Redis;

namespace KylinService.Redis.Push
{
    /// <summary>
    /// Redis配置管理
    /// </summary>
    public sealed class PushRedisConfigManager : IConfigurationSectionHandler
    {
        /// <summary>
        /// 配置项
        /// </summary>
        public static PushRedisCollection Collection { get; private set; }

        static PushRedisConfigManager()
        {
            ConfigurationManager.GetSection("pushRedisConfig");
        }

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            var _collection = new PushRedisCollection();

            string connectionString = null;

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var text = node.InnerText;

                    switch (node.Name.ToLower())
                    {
                        case "redisconnection":
                            connectionString = text;
                            break;
                        case "datakey":
                            var config = GetKeysDbConfigValue(node);
                            _collection[config.Key] = config;
                            break;
                    }
                }
            }

            _collection.Items.ForEach((item) =>
            {
                item.ConnectionString = connectionString;
            });

            Collection = _collection;

            return Collection;
        }

        /// <summary>
        /// 从配置中读取Redis中存储的数据Key及所在数据库序号
        /// </summary>
        /// <param name="config"></param>
        /// <param name="node"></param>
        private static PushKeysConfig GetKeysDbConfigValue(XmlNode node)
        {
            var name = GetAttributeValue(node, "name");

            var saveType = (RedisPushType)System.Enum.Parse(typeof(RedisPushType), name);

            int db = 0;

            int.TryParse(GetAttributeValue(node, "databaseIndex"), out db);

            var key = GetAttributeValue(node, "redisKey");

            return new PushKeysConfig
            {
                SaveType = saveType,
                DbIndex = db,
                Key = key
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

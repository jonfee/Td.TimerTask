using KylinService.SysEnums;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;

namespace KylinService.Redis
{
    /// <summary>
    /// Redis配置管理
    /// </summary>
    public class RedisConfigManager : IConfigurationSectionHandler
    {
        /// <summary>
        /// 配置项
        /// </summary>
        public static RedisConfig Config { get; private set; }

        static RedisConfigManager()
        {
            ConfigurationManager.GetSection("RedisConfig");
        }

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            Config = new RedisConfig();

            var keyList = new List<KeysConfig>();

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var text = node.InnerText;

                    switch (node.Name.ToLower())
                    {
                        case "redisconnection":
                            Config.ConnectString = text;
                            break;
                        case "datakey":
                            var temp = GetKeysDbConfigValue(node);
                            keyList.Add(temp);
                            break;
                    }
                }
            }

            Config.DbKeys = keyList;

            return Config;
        }

        /// <summary>
        /// 获取指定推送类型的接口信息
        /// </summary>
        /// <param name="pushType"></param>
        /// <returns></returns>
        public static KeysConfig GetSaveKeyDbConfig(RedisSaveType saveType)
        {
            return Config.DbKeys.Where(p => p.SaveType == saveType).FirstOrDefault();
        }

        /// <summary>
        /// 从配置中读取Redis中存储的数据Key及所在数据库序号
        /// </summary>
        /// <param name="config"></param>
        /// <param name="node"></param>
        private static KeysConfig GetKeysDbConfigValue(XmlNode node)
        {
            var name = GetAttributeValue(node, "name");

            var saveType = (RedisSaveType)System.Enum.Parse(typeof(RedisSaveType), name);

            int db = 0;

            int.TryParse(GetAttributeValue(node, "db"), out db);

            var key = GetAttributeValue(node, "key");

            return new KeysConfig
            {
                SaveType = saveType,
                DBindex = db,
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

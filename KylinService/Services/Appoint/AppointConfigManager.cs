using System.Configuration;
using System.Xml;

namespace KylinService.Services.Appoint
{
    public class AppointConfigManager : IConfigurationSectionHandler
    {
        /// <summary>
        /// 配置项
        /// </summary>
        public static AppointConfig Config { get; private set; }

        static AppointConfigManager()
        {
            ConfigurationManager.GetSection("AppointConfig");
        }

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            Config = new AppointConfig();

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var text = node.InnerText;

                    switch (node.Name.ToLower())
                    {
                        case "paymentwaitminutes":
                            int minutes = 0;
                            int.TryParse(text, out minutes);
                            Config.PaymentWaitMinutes = minutes;
                            break;
                        case "endservicewaituserdays":
                            int days = 0;
                            int.TryParse(text, out days);
                            Config.EndServiceWaitUserDays = days;
                            break;
                    }
                }
            }
            return Config;
        }
    }
}

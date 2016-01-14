using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KylinService.Services.MallOrderLate
{
    /// <summary>
    /// 自营商城订单逾期配置管理
    /// </summary>
    public class MallOrderLateConfigManager : IConfigurationSectionHandler
    {
        /// <summary>
        /// 配置项
        /// </summary>
        public static OrderLateConfig Config { get; set; }

        static MallOrderLateConfigManager()
        {
            ConfigurationManager.GetSection("MallOrderLateConfig");
        }

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            Config = new OrderLateConfig();

            foreach(XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var text = node.InnerText;

                    switch (node.Name.ToLower())
                    {
                        case "paymentwaithours":
                            int hours = 0;
                            int.TryParse(text, out hours);
                            Config.WaitPaymentHours = hours;
                            break;
                        case "receiptgoodswaitdays":
                            int days = 0;
                            int.TryParse(text, out days);
                            Config.WaitReceiptGoodsDays = days;
                            break;
                    }
                }
            }
            return Config;
        }
    }
}

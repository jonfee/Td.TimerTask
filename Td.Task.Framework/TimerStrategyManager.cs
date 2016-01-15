using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Td.Task.Framework
{
    /// <summary>
    /// 定时器配置管理
    /// </summary>
    public class TimerStrategyManager : IConfigurationSectionHandler
    {
        /// <summary>
        /// 定时任务策略配置
        /// </summary>
        public static TimerStrategyConfig StrategyConfig { get; private set; }

        static TimerStrategyManager()
        {
            ConfigurationManager.GetSection("TimerStrategyConfig");
        }

        /// <summary>
        /// 读取自定义配置节点
        /// </summary>
        /// <param name="parent">父结点</param>
        /// <param name="configContext">配置上下文</param>
        /// <param name="section">配置区</param>
        /// <returns></returns>
        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            StrategyConfig = new TimerStrategyConfig();

            var config = new List<TimerStrategy>();

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    switch (node.Name.ToLower())
                    {
                        case "default":
                            StrategyConfig.Default = node.InnerText;
                            break;
                        case "timerstrategy":
                            var temp = new TimerStrategy();
                            SetTimerConfigValue(temp, node);
                            config.Add(temp);
                            break;
                    }
                }
            }

            StrategyConfig.Config = config.ToArray();

            return StrategyConfig;
        }

        /// <summary>
        /// 从配置中读取并设置定时器值
        /// </summary>
        /// <param name="config"></param>
        /// <param name="node"></param>
        private void SetTimerConfigValue(TimerStrategy config, XmlNode node)
        {
            var datetimes = new List<DateTime>();
            var times = new List<TimeSpan>();

            foreach (XmlNode xn in node.ChildNodes)
            {
                if (xn.NodeType == XmlNodeType.Element)
                {
                    var text = xn.InnerText;

                    switch (xn.Name.ToLower())
                    {
                        case "refname":
                            config.RefName = text;
                            break;
                        case "reentrant":
                            config.ReEntrant = string.Compare(text, "true", true) == 0;
                            break;
                        case "timermode":
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                config.TimerMode = (TimerMode)Enum.Parse(typeof(TimerMode), text);
                            }
                            break;
                        case "delay":
                            long delayTemp = 0;
                            long.TryParse(text, out delayTemp);
                            config.Delay = new TimeSpan(delayTemp * 10000L);
                            break;
                        case "interval":
                            long intervalTemp = 0;
                            long.TryParse(text, out intervalTemp);
                            if (config.StrategyByDay != null)
                            {
                                config.StrategyByDay.Interval = new TimeSpan(intervalTemp*10000L);
                            }
                            else
                            {
                                config.Interval = new TimeSpan(intervalTemp * 10000L);
                            }
                            break;
                        case "times":
                            SetTimerConfigValue(config, xn);
                            break;
                        case "time":
                            var dt = DateTime.Parse(text);
                            datetimes.Add(dt);
                            break;
                        case "strategybyday":
                            //还是用这个函数处理下一级的配置
                            config.StrategyByDay = new TimerStrategyByDay();
                            SetTimerConfigValue(config, xn);    // 设置时间策略
                            break;
                        case "begintime":
                            if (String.IsNullOrEmpty(xn.InnerText)) xn.InnerText = "00:00:00";
                            config.StrategyByDay.BeginTime = TimeSpan.Parse(xn.InnerText);
                            break;
                        case "endtime":
                            if (String.IsNullOrEmpty(xn.InnerText)) xn.InnerText = "00:00:00";
                            config.StrategyByDay.EndTime = TimeSpan.Parse(xn.InnerText);
                            break;
                        case "timepoints":
                            SetTimerConfigValue(config, xn);    // 设置时间策略
                            break;
                        case "timevalue":
                            TimeSpan time = new TimeSpan(0);
                            if (TimeSpan.TryParse(text, out time))
                            {
                                times.Add(time);
                            }
                            break;
                    }
                }
            }

            if (times.Count != 0)
            {
                config.StrategyByDay.TimePoints = times.ToArray();
            }

            if (datetimes.Count != 0)
            {
                config.OnTimes = SortAndDistinct(datetimes);
            }
        }

        #region 排序和去重
        /// <summary>
        /// 升排序和去重
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<int> SortAndDistinct(List<int> values)
        {
            var list = new List<int>();
            values.Sort((x, y) => x - y);
            var tmp = -1;
            foreach (var intT in values)
            {
                if (tmp != intT)    // 目标是为了去重
                {
                    list.Add(intT);
                    tmp = intT;
                }
            }

            list.TrimExcess();
            return list;
        }

        /// <summary>
        /// 升排序和去重
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<DateTime> SortAndDistinct(List<DateTime> values)
        {
            var list = new List<DateTime>();
            values.Sort((x, y) => { return x < y ? -1 : 1; });
            var dt = DateTime.MaxValue;
            foreach (var intT in values)
            {
                if (dt != intT) // 目标是为了去重
                {
                    list.Add(intT);
                    dt = intT;
                }
            }

            list.TrimExcess();
            return list;
        }

        #endregion
    }
}

using KylinService.SysEnums;
using System.Collections.Generic;
using System.Linq;
using static KylinService.Core.EnumExtensions;

namespace KylinService.Core
{
    /// <summary>
    /// 系统数据
    /// </summary>
    public class SysData
    {
        private static List<EnumDesc> _serviceList = null;

        /// <summary>
        /// 服务类型集合
        /// </summary>
        public static List<EnumDesc> ServiceList
        {
            get
            {
                if (null == _serviceList)
                {
                    _serviceList = typeof(ScheduleType).GetEnumDesc();
                }
                return _serviceList;
            }
        }

        /// <summary>
        /// 获取服务名称
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static string GetServiceName(int serviceType)
        {
            string name = string.Empty;

            if (null != _serviceList)
            {
                var item = ServiceList.FirstOrDefault(p => p.Value == serviceType);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        /// <summary>
        /// 获取服务名称
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static string GetServiceName(string serviceType)
        {
            string name = string.Empty;

            if (null != _serviceList)
            {
                var item = ServiceList.FirstOrDefault(p => p.Name == serviceType);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }
    }
}

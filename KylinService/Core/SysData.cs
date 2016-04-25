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
        #region 队列服务

        private static List<EnumDesc<QueueScheduleType>> _queueServiceList = null;

        /// <summary>
        /// 队列服务类型集合
        /// </summary>
        public static List<EnumDesc<QueueScheduleType>> QueueServiceList
        {
            get
            {
                if (null == _queueServiceList)
                {
                    _queueServiceList = typeof(QueueScheduleType).GetEnumDesc<QueueScheduleType>();
                }
                return _queueServiceList;
            }
        }

        /// <summary>
        /// 获取队列服务名称
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static string GetQueueServiceName(int serviceType)
        {
            string name = string.Empty;

            if (null != QueueServiceList)
            {
                var item = QueueServiceList.FirstOrDefault(p => p.Value == serviceType);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        /// <summary>
        /// 获取队列服务名称
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static string GetQueueServiceName(string serviceType)
        {
            string name = string.Empty;

            if (null != QueueServiceList)
            {
                var item = QueueServiceList.FirstOrDefault(p => p.Name == serviceType);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        #endregion

        #region 清理服务

        private static List<EnumDesc<ClearScheduleType>> _clearServiceList = null;

        /// <summary>
        /// 清理服务类型集合
        /// </summary>
        public static List<EnumDesc<ClearScheduleType>> ClearServiceList
        {
            get
            {
                if (null == _clearServiceList)
                {
                    _clearServiceList = typeof(ClearScheduleType).GetEnumDesc<ClearScheduleType>();
                }
                return _clearServiceList;
            }
        }

        /// <summary>
        /// 获取清理服务名称
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static string GetClearServiceName(int serviceType)
        {
            string name = string.Empty;

            if (null != _clearServiceList)
            {
                var item = ClearServiceList.FirstOrDefault(p => p.Value == serviceType);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        /// <summary>
        /// 获取清理服务名称
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static string GetClearServiceName(string serviceType)
        {
            string name = string.Empty;

            if (null != _clearServiceList)
            {
                var item = ClearServiceList.FirstOrDefault(p => p.Name == serviceType);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        #endregion

        #region 缓存周期时间单位

        private static List<EnumDesc<CacheTimeOption>> _cacheTimeOptionList = null;

        /// <summary>
        /// 缓存周期时间单位集合
        /// </summary>
        public static List<EnumDesc<CacheTimeOption>> CacheTimeOptionList
        {
            get
            {
                if (null == _cacheTimeOptionList)
                {
                    _cacheTimeOptionList = typeof(CacheTimeOption).GetEnumDesc<CacheTimeOption>();
                }
                return _cacheTimeOptionList;
            }
        }

        /// <summary>
        /// 获取队列服务名称
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string GetCacheTimeOptionName(int option)
        {
            string name = string.Empty;

            if (null != CacheTimeOptionList)
            {
                var item = CacheTimeOptionList.FirstOrDefault(p => p.Value == option);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        /// <summary>
        /// 获取队列服务名称
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string GetCacheTimeOptionName(string option)
        {
            string name = string.Empty;

            if (null != CacheTimeOptionList)
            {
                var item = CacheTimeOptionList.FirstOrDefault(p => p.Name == option);

                name = null != item ? item.Description : string.Empty;
            }

            return name;
        }

        #endregion
    }
}

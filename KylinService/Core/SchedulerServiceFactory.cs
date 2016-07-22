using KylinService.Services;
using System;
using System.Collections.Generic;

namespace KylinService.Core
{
    //未使用。后续优化会用到
    public static class SchedulerServiceFactory
    {
        #region 私有字段

        private static readonly Dictionary<Enum, IService> _services = new Dictionary<Enum, IService>();

        #endregion

        #region 公共方法

        public static void Register(Enum serviceType, IService service)
        {
            if(_services.ContainsKey(serviceType))
                throw new InvalidOperationException("该服务已注册，请勿重复注册！");

            _services[serviceType] = service;
        }

        public static IService GetService(Enum serviceType)
        {
            if(_services.ContainsKey(serviceType))
                return _services[serviceType];

            return null;
        }

        public static T GetService<T>(Enum serviceType) where T : class, IService
        {
            return GetService(serviceType) as T;
        }

        #endregion
    }
}

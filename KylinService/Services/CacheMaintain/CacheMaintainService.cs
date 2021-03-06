﻿using KylinService.Core;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Td.Kylin.DataCache;

namespace KylinService.Services.CacheMaintain
{
    /// <summary>
    /// 缓存维护服务
    /// </summary>
    public sealed class CacheMaintainService : SchedulerService
    {
        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        public CacheMaintainService(string serviceName)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            var levelList = EnumExtensions.GetEnumDesc<CacheLevel>(typeof(CacheLevel));

            foreach (var level in levelList)
            {
                //更新周期（以毫秒为单位）
                TimeSpan period = new TimeSpan(0);

                var levelConfig = Startup.CacheMaintainConfigs.FirstOrDefault(p => p.Level == level.EnumItem);

                if (null != levelConfig && levelConfig.PeriodTime > 0)
                {
                    int time = levelConfig.PeriodTime;

                    switch (levelConfig.TimeOption)
                    {
                        case SysEnums.CacheTimeOption.Day: period = new TimeSpan(time, 0, 0, 0); break;
                        case SysEnums.CacheTimeOption.Hour: period = new TimeSpan(0, time, 0, 0); break;
                        case SysEnums.CacheTimeOption.Minute: period = new TimeSpan(0, 0, time, 0); break;
                    }

                    ScheduleClocker clocker = new ScheduleClocker(new TimeSpan(0), period, Execute, level.EnumItem);

                    Schedulers.Add(level.Name, clocker);
                }
            }

            return true;
        }

        /// <summary>
        /// 执行任务（更新缓存）
        /// </summary>
        /// <param name="state"></param>
        protected override void Execute(object state)
        {
            try
            {
                string message = null;

                CacheLevel level = default(CacheLevel);

                if (state.GetType().IsEnum && state is CacheLevel)
                {
                    level = (CacheLevel)state;

                    string levelName = null;
                    switch (level)
                    {
                        case CacheLevel.Hight: levelName = "高级别"; break;
                        case CacheLevel.Lower: levelName = "低级别"; break;
                        case CacheLevel.Middel: levelName = "中等级别"; break;
                        case CacheLevel.Permanent: levelName = "持久级别"; break;
                    }

                    //获取本次需要更新的缓存
                    var list = CacheCollection.GetCacheList(level);

                    lock (Startup.uploadCacheObjectLock)
                    {
                        //更新当前级别的缓存
                        CacheCollection.Update(level);
                    }

                    if (null != list && list.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in list)
                        {
                            sb.AppendLine(string.Format("{0}缓存“{1}”已更新！", levelName,
                                EnumExtensions.GetDescription<CacheItemType>(item.ItemType)));
                        }
                        message = sb.ToString();
                    }
                    else
                    {
                        message = string.Format("{0}缓存没有需要更新的缓存项", levelName);
                    }
                }

                if (string.IsNullOrWhiteSpace(message)) message = "没有任何可更新的缓存项";

                RunLogger(message);
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        public override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    Schedulers.Dispose();
                }

                m_disposed = true;
            }
        }

        public override void Pause()
        {
            base.Pause();

            Schedulers.Dispose();
        }

        public override void Continue()
        {
            base.Continue();

            SingleRequest();
        }
    }
}

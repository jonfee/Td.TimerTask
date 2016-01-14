using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KylinService.Manager
{
    /// <summary>
    /// 服务运行时计时器管理
    /// </summary>
    public class ClockerManager
    {
        volatile static ClockerManager _instance = null;

        static readonly object _mylock = new object();

        /// <summary>
        /// 服务运行计时器
        /// </summary>
        public volatile static List<Clocker> ClockerList;

        protected ClockerManager()
        {
            ClockerList = new List<Clocker>();
        }

        /// <summary>
        /// 服务运行时计时器管理－实例对象
        /// </summary>
        public static ClockerManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_mylock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ClockerManager();
                        }
                    }
                }

                return _instance;
            }
        }//end Instance

        /// <summary>
        /// 计时器Keys
        /// </summary>
        public List<string> ClockerKeys
        {
            get
            {
                if (null != ClockerList)
                {
                    return ClockerList.Select(p => p.Key).ToList();
                }
                return new List<string>();
            }
        }

        /// <summary>
        /// 添加一个服务运行计时器
        /// </summary>
        /// <param name="clocker"></param>
        public void Add(Clocker clocker)
        {
            if (null == clocker) return;

            lock (ClockerList)
            {
                if (ClockerList == null) ClockerList = new List<Clocker>();

                if (!ClockerKeys.Contains(clocker.Key))
                {
                    ClockerList.Add(clocker);
                }

                ClockerList.TrimExcess();
            }
        }

        /// <summary>
        /// 停止服务运行
        /// 停止计时器并释放计时器资源
        /// 从列表中移除
        /// </summary>
        /// <param name="clocker"></param>
        public void Stop(Clocker clocker)
        {
            if (null != clocker)
            {
                lock (ClockerList)
                {
                    clocker.RunningTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    clocker.RunningTimer.Dispose();

                    if (ClockerList.Contains(clocker))
                    {
                        ClockerList.Remove(clocker);
                    }

                    ClockerList.TrimExcess();
                }
            }
        }

        /// <summary>
        /// 停止服务运行
        /// </summary>
        /// <param name="key"></param>
        public void Stop(string key)
        {
            lock (ClockerList)
            {
                if (ClockerKeys.Contains(key))
                {
                    var clocker = ClockerList.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

                    if (null != clocker)
                    {
                        clocker.RunningTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        clocker.RunningTimer.Dispose();

                        ClockerList.Remove(clocker);

                        ClockerList.TrimExcess();
                    }
                }
            }
        }
    }
}

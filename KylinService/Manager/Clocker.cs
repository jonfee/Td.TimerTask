using System;

namespace KylinService.Manager
{
    /// <summary>
    /// 服务运行时计时器
    /// </summary>
    public class Clocker
    {
        public Clocker(string key, Action<object> action)
        {
            this.Key = key;

            this.StartTime = DateTime.Now;

            this.RunningTimer = new System.Threading.Timer(new System.Threading.TimerCallback(action), StartTime, 0, 1000);
        }

        /// <summary>
        /// 计时器Key（服务名称）
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 运行的定时器
        /// </summary>
        public System.Threading.Timer RunningTimer { get; private set; }

        /// <summary>
        /// 开始运行时间
        /// </summary>
        public DateTime StartTime { get; private set; }
    }
}

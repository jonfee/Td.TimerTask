using KylinService.Core;
using KylinService.Data.Provider;
using KylinService.SysEnums;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services.Clear.Shake
{
    /// <summary>
    /// 摇一摇服务
    /// </summary>
    public sealed class ShakeService : ClearSchedulerService
    {
        /// <summary>
        /// 定时器
        /// </summary>
        System.Threading.Timer timer;

        public ShakeService() : base(ClearScheduleType.ShakeDayTimesClear)
        {
            timer = new System.Threading.Timer(new TimerCallback(Execute), null, Timeout.Infinite, Timeout.Infinite);
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
                    timer.Dispose();
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="state"></param>
        protected override void Execute(object state)
        {
            try
            {
                int count = ShakeProvider.ResetDayTimes();

                string message = string.Format("共对 {0} 位用户进行了摇一摇当日已摇次数清除", count);

                Logger(message);
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }
        }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected override bool SingleRequest()
        {
            int duetime = 0;    //延迟时间量（以毫秒为单位）
            int period = 24 * 60 * 60 * 1000;   //间隔/周期时间量（以毫秒为单位）,此业务需求为每天执行

            //延迟时间就为：
            //  1、若当前时间为00:00:00（可偏移60秒），则立即执行
            //  2、其它时间，则延迟时间为当前时间距离第二天00:00:00的时间量
            var now = DateTime.Now;
            if (now.Subtract(now.Date).TotalMinutes > 1)
            {
                duetime = (int)now.Date.AddDays(1).Subtract(now).TotalMilliseconds;
            }
            timer.Change(duetime, period);

            return true;
        }

        public override void Pause()
        {
            base.Pause();

            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public override void Continue()
        {
            base.Continue();

            SingleRequest();
        }
    }
}

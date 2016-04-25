using KylinService.Core;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services
{
    /// <summary>
    /// 自定义任务定时器
    /// </summary>
    public class ScheduleClocker : IDisposable
    {
        /// <summary>
        /// 初始化一个任务定时器
        /// </summary>
        /// <param name="duetime">定时器启动后延迟执行callback的时间</param>
        /// <param name="period">间隔执行callback的周期</param>
        /// <param name="callback">回调方法（Action委托）</param>
        public ScheduleClocker(TimeSpan duetime, TimeSpan period, Action<object> callback) : this(duetime, period, callback, null) { }

        /// <summary>
        /// 初始化一个任务定时器
        /// </summary>
        /// <param name="duetime">定时器启动后延迟执行callback的时间</param>
        /// <param name="period">间隔执行callback的周期</param>
        /// <param name="callback">回调方法（Action委托）</param>
        /// <param name="callbackData">回调方法所需要的参数值</param>
        public ScheduleClocker(TimeSpan duetime, TimeSpan period, Action<object> callback, object callbackData)
        {
            _dueTime = duetime;
            Period = period;
            Callback = callback;

            TimeSpan clockerPeriod = new TimeSpan(0, 0, 1);

            _timer = new System.Threading.Timer((state) =>
              {
                  if (_dueTime.Ticks > 0)
                  {
                      _dueTime = _dueTime.Subtract(clockerPeriod);

                      if (_dueTime.Ticks < 0) _dueTime = new TimeSpan(0);
                  }

                  bool isTimeout = false;

                  if (PrevExecuteTime.HasValue)
                  {
                      isTimeout = PrevExecuteTime.Value.Add(Period) <= DateTime.Now;
                  }
                  else//首次启动，尚未执行
                  {
                      _dueTime = _dueTime.Subtract(clockerPeriod);

                      if (_dueTime.Ticks < 0)
                      {
                          _dueTime = new TimeSpan(0);
                          isTimeout = true;
                      }
                  }

                  if (isTimeout)
                  {
                      PrevExecuteTime = DateTime.Now;

                      callback(callbackData);
                  }

              }, null, new TimeSpan(0), clockerPeriod);
        }

        #region 成员变量

        /// <summary>
        /// 计时器
        /// </summary>
        private System.Threading.Timer _timer;

        /// <summary>
        /// 启动剩余延迟时间
        /// </summary>
        private TimeSpan _dueTime;

        /// <summary>
        /// 任务执行周期
        /// </summary>
        public TimeSpan Period { get; private set; }

        private DateTime? _prevExecuteTime;
        /// <summary>
        /// 上次执行任务的时间
        /// </summary>
        public DateTime? PrevExecuteTime
        {
            get { return _prevExecuteTime; }
            set { _prevExecuteTime = value; }

        }

        #endregion

        Action<object> Callback;

        public void Dispose()
        {
            _timer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}

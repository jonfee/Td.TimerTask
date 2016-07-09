using KylinService.Core;
using KylinService.Core.Loger;
using System;
using System.Text;
using System.Threading;

namespace KylinService.Services
{
    /// <summary>
    /// 任务计划服务抽象类
    /// </summary>
    public abstract class SchedulerService : IService
    {
        /// <summary>
        /// 是否已释放
        /// </summary>
        protected bool m_disposed;

        /// <summary>
        /// 任务计划收集器
        /// </summary>
        protected SchedulerCollection Schedulers;

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName;

        /// <summary>
        /// Timeout.Infinite
        /// </summary>
        public TimeSpan TimeoutInfinite { get { return new TimeSpan(0, 0, 0, 0, -1); } }

        /// <summary>
        /// 表示0时间刻度
        /// </summary>
        public TimeSpan TimeoutZero { get { return new TimeSpan(0); } }

        /// <summary>
        /// 是否已暂停（暂停服务，但不影响已经开始工作的任务执行）
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// 是否循环处理
        /// </summary>
        private bool _isLoop;

        /// <summary>
        /// 循环队列为空时默认等待下次执行的时间（毫秒）间隔，如连续为empty，等待时间为当前设置时间的倍数
        /// </summary>
        private int _defaultNextWaitMillisecondsIfEmpty;

        /// <summary>
        /// 队列为空时阶梯等待时间的最大倍数
        /// </summary>
        private const int _maxWaitTimes = 20;

        /// <summary>
        /// 当前已等待时间倍数
        /// </summary>
        private int _waitTimes = 0;

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public SchedulerService() : this(isLoop: false, defaultNextWaitMillisecondsIfEmpty: 0) { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        /// <param name="isLoop">是否循环处理</param>
        /// <param name="defaultNextWaitMillisecondsIfEmpty">循环队列为空时默认等待下次执行的时间（毫秒）间隔，如连续为empty，等待时间为当前设置时间的倍数</param>
        public SchedulerService(bool isLoop = false, int defaultNextWaitMillisecondsIfEmpty = 0)
        {
            Schedulers = new SchedulerCollection();

            this._isLoop = isLoop;

            this._defaultNextWaitMillisecondsIfEmpty = defaultNextWaitMillisecondsIfEmpty;
        }

        #endregion

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        //释放
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing)
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

        /// <summary>
        /// 服务启动
        /// </summary>
        public void Start()
        {
            this.IsPaused = false;

            OnStart();
        }

        /// <summary>
        /// 开始处理
        /// </summary>
        private void OnStart()
        {
            ThreadPool.QueueUserWorkItem((item) =>
            {
                if (!_isLoop)
                {
                    SingleMain();
                }
                else
                {
                    while (true)
                    {
                        bool onContinue = false;

                        //未暂停时
                        if (!IsPaused)
                        {
                            //执行单次请求并返回是否需要继续信号指示
                            onContinue = SingleMain();
                        }

                        //不继续时
                        if (!onContinue)
                        {
                            if (_waitTimes < _maxWaitTimes)
                            {
                                _waitTimes++;
                            }

                            //休眠时间（单位：毫秒）
                            var waitMilliseconds = _defaultNextWaitMillisecondsIfEmpty * _waitTimes;

                            //休眠一段时间（单位：毫秒），避免CPU空转
                            Thread.Sleep(waitMilliseconds);
                        }
                        else
                        {
                            _waitTimes = 0;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 单一操作执行主程序
        /// </summary>
        /// <returns></returns>
        private bool SingleMain()
        {
            bool _continue = false;
            try
            {
                //执行单次请求并返回是否需要继续指示信号
                _continue = SingleRequest();
            }
            catch (Exception ex)
            {
                OnThrowException(ex);
            }

            return _continue;
        }

        /// <summary>
        /// 执行单次请求并返回是否需要继续指示信号
        /// </summary>
        /// <returns></returns>
        protected abstract bool SingleRequest();

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="state"></param>
        protected abstract void Execute(object state);

        /// <summary>
        /// 暂停服务（IsPaused=true）
        /// </summary>
        public virtual void Pause()
        {
            this.IsPaused = true;
        }

        /// <summary>
        /// 继续服务（IsPaused=false）
        /// </summary>
        public virtual void Continue()
        {
            this.IsPaused = false;
        }

        /// <summary>
        /// 异常 处理
        /// </summary>
        /// <param name="ex"></param>
        protected void OnThrowException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("出错了，原因：{0}", ex.Message));

            if (ex is CustomException)
            {
                Logger(sb.ToString());
            }
            else
            {
                sb.AppendLine("异常详情：");
                sb.AppendLine(ex.StackTrace);

                //写入异常日志
                var loger = new ExceptionLoger();
                loger.Write(ServiceName, ex);
            }
        }

        /// <summary>
        /// 输出消息并记录日志
        /// </summary>
        /// <param name="message"></param>
        protected void Logger(string message)
        {
            WriteMessageHelper.WriteMessage(message);

            RunLoger loger = new RunLoger(ServiceName);
            loger.Write(message);
        }
    }
}

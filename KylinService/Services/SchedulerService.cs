using KylinService.Core;
using KylinService.Core.Loger;
using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
        /// 消息输出委托
        /// </summary>
        private DelegateTool.WriteMessageDelegate WriteDelegate;

        /// <summary>
        /// 当前操作Form窗体
        /// </summary>
        private Form CurrentForm;

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
        private const int _maxWaitTimes = 10;

        /// <summary>
        /// 当前已等待时间倍数
        /// </summary>
        private int _waitTimes = 0;

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public SchedulerService() : this(null, null) { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        /// <param name="isLoop">是否循环处理</param>
        /// <param name="defaultNextWaitMillisecondsIfEmpty">循环队列为空时默认等待下次执行的时间（毫秒）间隔，如连续为empty，等待时间为当前设置时间的倍数</param>
        public SchedulerService(Form form, DelegateTool.WriteMessageDelegate writeDelegate, bool isLoop = false, int defaultNextWaitMillisecondsIfEmpty = 0)
        {
            Schedulers = new SchedulerCollection();

            this.CurrentForm = form;

            this.WriteDelegate = writeDelegate;

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
                    ServiceMain();
                }
                else
                {
                    while (true)
                    {
                        bool onContinue = false;

                        //未暂停时
                        if (!IsPaused)
                        {
                            //执行单次请求并返回是否需要继续指示信号
                            onContinue = ServiceMain();
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
        /// 服务主程序执行
        /// </summary>
        /// <returns></returns>
        private bool ServiceMain()
        {
            bool _continue = false;
            try
            {
                //执行单次请求并返回是否需要继续指示信号
                _continue = SingleRequest();

                //不能继续时写入脏数据
                if (!_continue)
                {
                    WriteDirtyData();
                }
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
        /// 写入脏数据
        /// </summary>
        protected abstract void WriteDirtyData();

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="state"></param>
        protected abstract void Execute(object state);

        /// <summary>
        /// 暂停服务
        /// </summary>
        public virtual void Pause()
        {
            this.IsPaused = true;
            OnStart();
        }

        /// <summary>
        /// 继续服务
        /// </summary>
        public void Continue()
        {
            this.IsPaused = false;
            OnStart();
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
                OutputMessage(sb.ToString());
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
        /// 输出消息
        /// </summary>
        /// <param name="message"></param>
        protected void OutputMessage(string message)
        {
            DelegateTool.WriteMessage(CurrentForm, WriteDelegate, message);

            RunLoger loger = new RunLoger(ServiceName);
            loger.Write(message);
        }
    }
}

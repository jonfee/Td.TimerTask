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
        public bool IsPaused { get; protected set; }

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public SchedulerService()
        {
            Schedulers = new SchedulerCollection();
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
        public virtual void Start()
        {
            this.IsPaused = false;

            ThreadPool.QueueUserWorkItem((item) =>
            {
                SingleMain();
            });
        }

        /// <summary>
        /// 单一操作执行主程序
        /// </summary>
        /// <returns></returns>
        protected bool SingleMain()
        {
            bool _continue = false;
            try
            {
                //执行单次请求并返回是否需要继续指示信号
                _continue = SingleRequest();
            }
            catch (Exception ex)
            {
                if (ex.StackTrace.Contains("StackExchange.Redis"))
                {
                    RedisException(ex);
                }
                else
                {
                    OnThrowException(ex);
                }
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
        /// redis异常日志记录
        /// </summary>
        /// <param name="ex"></param>
        protected void RedisException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("出错了，原因：{0}", ex.Message));

            sb.AppendLine("异常详情：");
            sb.AppendLine(ex.StackTrace);

            //写入异常日志
            string logfile = string.Format(@"\logs\redis-exception\{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            var loger = new ExceptionLoger(logfile);
            loger.Write(ServiceName, ex);
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
                string logfile = string.Format(@"\logs\default-exception\{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
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

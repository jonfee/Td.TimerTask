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
        public SchedulerService(Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            Schedulers = new SchedulerCollection();

            this.CurrentForm = form;

            this.WriteDelegate = writeDelegate;
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
        public abstract void OnStart();

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="state"></param>
        protected abstract void Execute(object state);

        /// <summary>
        /// 异常 处理
        /// </summary>
        /// <param name="ex"></param>
        protected void OnThrowException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("出错了，原因：{0}", ex.Message));
            sb.AppendLine("异常详情：");
            sb.AppendLine(ex.StackTrace);

            OutputMessage(sb.ToString());

            //写入异常日志
            var loger = new ExceptionLoger();
            loger.Write(ServiceName, ex);
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

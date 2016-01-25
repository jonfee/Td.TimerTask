using KylinService.Core;
using KylinService.Core.Loger;
using System;
using System.Threading;
using System.Windows.Forms;

namespace KylinService.Services
{
    /// <summary>
    /// 任务计划抽象基类
    /// </summary>
    public abstract class IScheduler
    {
        public IScheduler(Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            this.CurrentForm = form;

            this.WriteDelegate = writeDelegate;
        }

        /// <summary>
        /// 计划Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 定义一个线程
        /// </summary>
        public Thread CurrentThread { get; private set; }

        /// <summary>
        /// 执行程序
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// 当前窗体
        /// </summary>
        protected Form CurrentForm { get; private set; }

        /// <summary>
        /// 输出消息委托
        /// </summary>
        protected DelegateTool.WriteMessageDelegate WriteDelegate { get; private set; }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            ThreadStart myThreadDelegate = new ThreadStart(Execute);

            CurrentThread = new Thread(myThreadDelegate);

            CurrentThread.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (null != CurrentThread)
            {
                CurrentThread.Abort();
            }
        }
    }
}

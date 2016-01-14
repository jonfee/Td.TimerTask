using KylinService.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Td.Task.Framework;

namespace KylinService.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public class BaseService : ITask
    {
        public BaseService(Form form, DelegateTool.WriteMessageDelegate writeDelegate)
        {
            this.CurrentForm = form;

            this.WriteDelegate = writeDelegate;
        }

        /// <summary>
        /// 消息输出委托
        /// </summary>
        protected DelegateTool.WriteMessageDelegate WriteDelegate { get; private set; }

        /// <summary>
        /// 当前操作Form窗体
        /// </summary>
        protected Form CurrentForm { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        protected string ServiceName { get; set; }

        protected override void OnStart(params object[] parameters)
        {
            string message = string.Format("{0} 计划已启动，但没有加载任何处理程序！", ServiceName);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, message);
        }

        protected override void OnStop()
        {
            string message = string.Format("{0} 计划已停止！", ServiceName);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, message);
        }

        protected override void OnThrowException(Exception ex)
        {
            string message = string.Format("出错啦！！！原因：{0}", ex.Message);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, message);
        }
    }
}

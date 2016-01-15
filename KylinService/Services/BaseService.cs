using KylinService.Core;
using KylinService.Core.Loger;
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
        /// 服务类型
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        protected string ServiceName { get { return SysData.GetServiceName(ServiceType); } }

        protected override void OnStart(params object[] parameters)
        {
            string message = string.Format("{0} 服务已启动！", ServiceName);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, message);

            //记录启动日志
            var loger = new ServerLoger(ServiceName);
            loger.Write("服务已启动！");
        }

        protected override void OnStop()
        {
            string message = string.Format("{0} 服务已停止！", ServiceName);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, message);

            //记录启动日志
            var loger = new ServerLoger(ServiceName);
            loger.Write("服务已停止！");
        }

        protected override void OnThrowException(Exception ex)
        {
            string message = string.Format("出错啦！！！原因：{0}", ex.Message);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, message);

            //写入异常日志
            var loger = new ExceptionLoger();
            loger.Write(ServiceName, ex);
        }
    }
}

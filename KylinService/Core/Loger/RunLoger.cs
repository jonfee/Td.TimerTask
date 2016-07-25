using System;
using System.Text;

namespace KylinService.Core.Loger
{
    /// <summary>
    /// 运行日志
    /// </summary>
    public class RunLoger : BaseLoger
    {
        public RunLoger() : base(string.Format(@"\logs\output\{0}.txt", DateTime.Now.ToString("yyyyMMdd"))) { }

        public RunLoger(string serviceName) : base(string.Format(@"\logs\{0}\{1}.txt", serviceName, DateTime.Now.ToString("yyyyMMdd"))) { }

        /// <summary>
        /// 服务操作日志
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            StringBuilder sbContent = new StringBuilder();

            sbContent.AppendLine(string.Format("{0}-------------{1}", message, DateTime.Now));

            base.LogWrite(sbContent);
        }
    }
}

using System.Text;

namespace KylinService.Core.Loger
{
    public class ServerLoger : BaseLoger
    {
        public ServerLoger(string serviceName) : base(@"\logs\service.txt")
        {
            this.serviceName = serviceName;
        }

        private string serviceName;

        /// <summary>
        /// 服务操作日志
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            StringBuilder sbContent = new StringBuilder();

            sbContent.Append("________________________________________________________________________________________________________________\r\n\r\n");
            sbContent.Append("日期：" + System.DateTime.Now.ToString() + "\r\n");
            sbContent.Append("服务名称：" + serviceName + "\r\n");
            sbContent.Append("操作信息：" + message + "\r\n");
            sbContent.Append("________________________________________________________________________________________________________________\r\n");

            base.LogWrite(sbContent);
        }
    }
}

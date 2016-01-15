using System;
using System.Text;

namespace KylinService.Core.Loger
{
    /// <summary>
    /// 异常日志
    /// </summary>
    public class ExceptionLoger : BaseLoger
    {
        public ExceptionLoger() : base(@"\logs\exception.txt") { }

        /// <summary>
        /// 异常信息日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="ex"></param>
        public void Write(string title, Exception ex)
        {
            StringBuilder sbContent = new StringBuilder();

            sbContent.Append("\r\n________________________________________________________________________________________________________________\r\n");
            sbContent.Append("日期：" + System.DateTime.Now.ToString() + "\r\n");
            sbContent.Append("标题：" + title + "\r\n");
            sbContent.Append("异常信息：" + ex.Message + "\r\n");
            sbContent.Append("异常内容：" + ex.StackTrace + "\r\n");
            sbContent.Append("\r\n________________________________________________________________________________________________________________\r\n");

            base.LogWrite(sbContent);
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KylinService.Core.Loger
{
    /// <summary>
    /// 日志记录基类
    /// </summary>
    public abstract class BaseLoger
    {
        public BaseLoger(string filepath)
        {
            this.appFilePath = Application.StartupPath + filepath;
        }

        /// <summary>
        /// 基于Application的文件路径 
        /// </summary>
        private string appFilePath;

        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="content"></param>
        protected void LogWrite(StringBuilder content)
        {
            if (null == content) content = new StringBuilder();

            LogWrite(content.ToString());
        }

        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="content"></param>
        protected void LogWrite(string content)
        {
            try
            {
                string lastFolderPath = appFilePath.Remove(appFilePath.LastIndexOf(@"\"));

                if (!Directory.Exists(lastFolderPath))
                {
                    Directory.CreateDirectory(lastFolderPath);
                }

                if (!File.Exists(appFilePath))
                {
                    File.Create(appFilePath).Close();
                }

                using (StreamWriter sw = File.AppendText(appFilePath))
                {
                    sw.Write(content);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch
            {
                //异常处理
            }
        }
    }
}

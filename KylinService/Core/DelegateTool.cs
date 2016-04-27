using System;
using System.Windows.Forms;

namespace KylinService.Core
{
    public class DelegateTool
    {
        /// <summary>
        /// 消息输出委托
        /// </summary>
        /// <param name="message"></param>
        public delegate void WriteMessageDelegate(string message, bool padTime = true);

        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public static void WriteMessage(Form form, WriteMessageDelegate writeDelegate, string message)
        {
            if (null != form && null != writeDelegate)
            {
                form.Invoke((EventHandler)delegate
                {
                    writeDelegate(message, true);
                });
            }
        }
    }
}

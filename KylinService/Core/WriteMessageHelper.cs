using System;
using System.Windows.Forms;

namespace KylinService.Core
{
    public sealed class WriteMessageHelper
    {
        /// <summary>
        /// 输出消息的Form对象实例
        /// </summary>
        public static Form OutputForm;

        public static WriteMessageDelegate OutputMessage;

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
        public static void WriteMessage(string message)
        {
            if (null != OutputForm && null != OutputMessage)
            {
                OutputForm.Invoke((EventHandler)delegate
                {
                    OutputMessage(message, true);
                });
            }
        }
    }
}

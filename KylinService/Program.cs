using System;
using System.Windows.Forms;

namespace KylinService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Startup.Init();

            Application.Run(new MainForm());
        }
    }
}

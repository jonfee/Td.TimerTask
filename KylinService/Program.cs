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

            var mainForm = new MainForm();

            //// 注册所有调度服务。
            //Startup.RegisterServices(mainForm, mainForm.WriteMessageDelegate);

            Application.Run(mainForm);
        }
    }
}

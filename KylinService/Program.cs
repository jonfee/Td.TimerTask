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

            //初始化信息
            Startup.Start();

            //启动主窗体
            var mainForm = new MainForm();

            //启动一个定时器，每10分钟自动从缓存中更新数据 
            var timer = new System.Threading.Timer((state) =>
              {
                  lock (Startup.uploadCacheObjectLock)
                  {
                      //更新相关配置
                      Startup.UpdateQueueConfig();
                  }
              }, null, 0, 600000);

            //// 注册所有调度服务。
            //Startup.RegisterServices(mainForm, mainForm.WriteMessageDelegate);

            Application.Run(mainForm);
        }
    }
}

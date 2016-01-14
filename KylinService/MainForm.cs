using KylinService.Services;
using KylinService.SysEnums;
using System.Windows.Forms;
using Td.Task.Framework;
using KylinService.Core;
using System.Linq;
using System;
using System.Threading;
using KylinService.Manager;

namespace KylinService
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Init();
        }

        private string _serTname = "lbServer_";     //服务类型名称展示控件标识
        private string _serTstatus = "lb_Status_";  //服务运行状态展示控件标识
        private string _serTstartbtn = "btnStart_"; //服务启用按钮标识
        private string _serTstopbtn = "btnStop_";   //服务停止按钮标识
        private string _serTtime = "time_";         //服务运行时间控件标识

        //定义消息输出委托
        private DelegateTool.WriteMessageDelegate writeDelegate;

        /// <summary>
        /// 初始化服务项及窗体控件
        /// </summary>
        private void Init()
        {
            //委托
            writeDelegate = WriteMessage;

            var list = SysData.ServiceList;

            //遍历服务类型
            for (var i = 0; i < list.Count; i++)
            {
                var serv = list[i];

                #region//服务项面板Panel
                Panel panel = new Panel();
                panel.Width = (int)Math.Floor(groupSchedule.Width * 0.92);
                panel.Height = 30;
                var padLeft = (groupSchedule.Width - panel.Width) / 2;
                panel.Location = new System.Drawing.Point(padLeft, i * 30 + 30);
                #endregion

                #region//服务类型名称Lable
                Label lbType = new Label();
                lbType.Width = 180;
                lbType.Text = serv.Description;
                lbType.Name = _serTname + serv.Name;
                lbType.Location = new System.Drawing.Point(0, 5);
                panel.Controls.Add(lbType);
                #endregion

                #region//服务运行状态Label
                Label lbStatus = new Label();
                lbStatus.Width = 100;
                lbStatus.Text = "未运行";
                lbStatus.Name = _serTstatus + serv.Name;
                lbStatus.Location = new System.Drawing.Point(180, 5);
                panel.Controls.Add(lbStatus);
                #endregion

                #region//服务启动按钮
                Button startButton = new Button();
                startButton.Text = "启动";
                startButton.Name = _serTstartbtn + serv.Name;
                startButton.Tag = serv.Name;
                startButton.Location = new System.Drawing.Point(300, 0);
                startButton.Click += new EventHandler(BtnStart_Click);
                panel.Controls.Add(startButton);
                #endregion

                #region//服务停止按钮
                Button stopButton = new Button();
                stopButton.Text = "停止";
                stopButton.Name = _serTstopbtn + serv.Name;
                stopButton.Tag = serv.Name;
                stopButton.Location = new System.Drawing.Point(400, 0);
                stopButton.Enabled = false;
                stopButton.Click += new EventHandler(BtnStop_Click);
                panel.Controls.Add(stopButton);
                #endregion

                #region//运行时长标题说明Lable
                Label lbTimeTip = new Label();
                lbTimeTip.Width = 70;
                lbTimeTip.Text = "运行时长：";
                lbTimeTip.Location = new System.Drawing.Point(500, 5);
                panel.Controls.Add(lbTimeTip);
                #endregion

                #region//运行时长展示Label
                Label lbTime = new Label();
                lbTime.Width = 200;
                lbTime.Text = "0天 0时 0分 0秒";
                lbTime.Name = _serTtime + serv.Name;
                lbTime.Location = new System.Drawing.Point(570, 5);
                panel.Controls.Add(lbTime);
                #endregion

                this.groupSchedule.Controls.Add(panel);
            }
        }

        /// <summary>
        /// 服务启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnStart_Click(object sender, EventArgs e)
        {
            #region 界面及服务计时

            var btn = (Button)sender;

            var scheduleTypeName = (btn.Tag ?? string.Empty).ToString();

            //所有计划策略名
            var refNames = TimerStrategyManager.StrategyConfig.Config.Select(p => p.RefName).ToArray();

            if (refNames == null || !refNames.Contains(scheduleTypeName))
            {
                MessageBox.Show("未检测到有效的任务计划策略配置");
                return;
            }

            //禁用启动按钮
            btn.Enabled = false;

            //父控件
            var parent = btn.Parent;

            //找到停止按钮并启用交互
            var btnStop = Find<Button>(parent, _serTstopbtn, scheduleTypeName);
            btnStop.Enabled = true;

            //找到状态Label，并更新状态为：运行中
            var lbStatus = Find<Label>(parent, _serTstatus, scheduleTypeName);
            lbStatus.Text = "运行中";

            //找到显示服务名称的Label
            var lbServName = Find<Label>(parent, _serTname, scheduleTypeName);
            string msg = string.Format("{0} 服务已启动！************ {1}", lbServName.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteMessage(msg);

            //找到运行时间显示控件
            var lbTime = Find<Label>(parent, _serTtime, scheduleTypeName);

            //服务计时
            var clocker = new Clocker(scheduleTypeName, (obj) =>
              {
                  DateTime startTime = DateTime.Parse(obj.ToString());

                  var timespan = DateTime.Now - startTime;

                  var strTime = string.Format("{0}天 {1}小时 {2}分 {3}秒", timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds);

                  this.Invoke((EventHandler)delegate
                  {
                      lbTime.Text = strTime;
                  });
              });

            ClockerManager.Instance.Add(clocker);
            #endregion

            //当前服务的计划类型
            ScheduleType schedule = (ScheduleType)System.Enum.Parse(typeof(ScheduleType), scheduleTypeName);

            switch (schedule)
            {
                case ScheduleType.AppointOrderLate:
                    break;
                case ScheduleType.MallOrderLate:
                    break;
                case ScheduleType.ShakeDayTimesClear:
                    TaskSchedule.StartSchedule(scheduleTypeName, new Services.ShakeDayTimes.ShakeTimesClearService(this, writeDelegate), scheduleTypeName, null);
                    break;
                case ScheduleType.WelfareLottery:
                    TaskSchedule.StartSchedule(scheduleTypeName, new Services.WelfareLottery.WelfareLotteryService(this, writeDelegate), scheduleTypeName, null);
                    break;
            }
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnStop_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            var scheduleTypeName = (btn.Tag ?? string.Empty).ToString();

            btn.Enabled = false;

            //父控件
            var parent = btn.Parent;

            //找到启用按钮并启用交互
            var btnStart = Find<Button>(parent, _serTstartbtn, scheduleTypeName);
            btnStart.Enabled = true;

            //找到状态Label，并更新状态为：已停止
            var lbStatus = Find<Label>(parent, _serTstatus, scheduleTypeName);
            lbStatus.Text = "已停止";

            //找到显示服务名称的Label
            var lbServName = Find<Label>(parent, _serTname, scheduleTypeName);
            string msg = string.Format("{0} 服务已停止！************ {1}", lbServName.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteMessage(msg);

            ClockerManager.Instance.Stop(scheduleTypeName);
        }

        /// <summary>
        ///  获取控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">父控件</param>
        /// <param name="tagName">标记名称</param>
        /// <param name="scheduleType">任务计划类型名称</param>
        /// <returns></returns>
        private T Find<T>(Control parent, string tagName, string scheduleType) where T : Control
        {
            Type type = typeof(T);

            var controls = parent.Controls;

            foreach (Control item in controls)
            {
                string name = tagName + scheduleType;

                if (item.Name == name)
                {
                    return (T)item;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 定义委托变量
        /// </summary>
        public delegate void WriteMessageDelegate(string message);

        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="message"></param>
        private void WriteMessage(string message)
        {
            this.richMessage.AppendText(message);
            this.richMessage.AppendText("\n");
            this.richMessage.Focus();
        }
    }
}

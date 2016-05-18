using KylinService.Core;
using KylinService.Core.Loger;
using KylinService.Data.Settlement;
using KylinService.Services;
using KylinService.Services.CacheMaintain;
using KylinService.Services.Clear.Shake;
using KylinService.Services.Queue.Appoint;
using KylinService.Services.Queue.Circle;
using KylinService.Services.Queue.Mall;
using KylinService.Services.Queue.Merchant;
using KylinService.Services.Queue.Welfare;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Td.Kylin.DataCache;

namespace KylinService
{
    public partial class MainForm : Form
    {
        //这里在窗体上没有拖拽一个NotifyIcon控件，而是在这里定义了一个变量  
        private NotifyIcon notifyIcon = null;

        public MainForm()
        {
            InitializeComponent();

            Init();
        }

        /// <summary>
        /// 初始化服务项及窗体控件
        /// </summary>
        private void Init()
        {
            this.richMessage.ReadOnly = true;

            //初始化显示消息
            InitShow();

            //委托
            writeDelegate = WriteMessage;

            //初始化显示产品信息
            InitProductInfo();

            //初始化清理服务控件绑定
            InitClearControls();

            //初始化队列服务控制绑定
            InitQueueControls();

            //缓存维护服务控制绑定
            InitCacheControls();
        }

        ServiceCollection<SchedulerService> _serviceCollection = new ServiceCollection<SchedulerService>();

        #region /////////////////私有变量///////////////////

        private string _serTname = "lbServer_";     //服务类型名称展示控件标识
        private string _serTstatus = "lb_Status_";  //服务运行状态展示控件标识
        private string _serTstartbtn = "btnStart_"; //服务启用按钮标识
        private string _serTstopbtn = "btnStop_";   //服务停止按钮标识
        //private string _serTtime = "time_";         //服务运行时间控件标识

        //定义消息输出委托
        private DelegateTool.WriteMessageDelegate writeDelegate;

        #endregion

        #region  ////////////////////服务控件初始化与绑定///////////////////////////

        #region 清理服务
        /// <summary>
        /// 初始化清理服务
        /// </summary>
        private void InitClearControls()
        {
            var list = SysData.ClearServiceList;

            //遍历服务类型
            for (var i = 0; i < list.Count; i++)
            {
                var serv = list[i];

                #region//服务项面板Panel
                Panel panel = new Panel();
                panel.Width = (int)Math.Floor(tabClear.Width * 0.95);
                panel.Height = 30;
                var padLeft = (tabClear.Width - panel.Width) / 2;
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
                startButton.Click += new EventHandler(BtnClearStart_Click);
                panel.Controls.Add(startButton);
                #endregion

                #region//服务停止按钮
                Button stopButton = new Button();
                stopButton.Text = "停止";
                stopButton.Name = _serTstopbtn + serv.Name;
                stopButton.Tag = serv.Name;
                stopButton.Location = new System.Drawing.Point(400, 0);
                stopButton.Enabled = false;
                stopButton.Click += new EventHandler(BtnClearStop_Click);
                panel.Controls.Add(stopButton);
                #endregion

                this.tabClear.Controls.Add(panel);
            }
        }

        /// <summary>
        /// 清理服务启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClearStart_Click(object sender, EventArgs e)
        {
            #region 获取当前正在操作的服务类型

            var btn = (Button)sender;

            var serviceName = (btn.Tag ?? string.Empty).ToString();

            #endregion

            #region 启动服务

            //当前服务的计划类型
            ClearScheduleType schedule = (ClearScheduleType)Enum.Parse(typeof(ClearScheduleType), serviceName);

            //任务计划服务
            SchedulerService service = null;

            switch (schedule)
            {
                //摇一摇服务
                case ClearScheduleType.ShakeDayTimesClear:
                    service = new ShakeService(this, writeDelegate);
                    break;
            }

            #endregion

            #region 

            if (service != null)
            {
                service.OnStart();

                _serviceCollection.Add(serviceName, service);

                //禁用启动按钮
                btn.Enabled = false;

                //父控件
                var parent = btn.Parent;

                //找到停止按钮并启用交互
                var btnStop = Find<Button>(parent, _serTstopbtn, serviceName);
                btnStop.Enabled = true;

                //找到状态Label，并更新状态为：运行中
                var lbStatus = Find<Label>(parent, _serTstatus, serviceName);
                lbStatus.Text = "运行中";
                lbStatus.ForeColor = System.Drawing.Color.Green;

                //服务已启动
                var servName = SysData.GetClearServiceName(serviceName);

                string message = string.Format("【{0}】 服务已启动！", servName);
                WriteMessage(message);

                //记录启动日志
                var loger = new ServerLoger(servName);
                loger.Write("服务已启动！");
            }
            #endregion
        }

        /// <summary>
        /// 清理服务停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClearStop_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            var serviceName = (btn.Tag ?? string.Empty).ToString();

            #region 停止服务

            //当前服务的计划类型
            ClearScheduleType schedule = (ClearScheduleType)Enum.Parse(typeof(ClearScheduleType), serviceName);

            //任务计划服务
            SchedulerService service = _serviceCollection[serviceName];

            if (null != service)
            {
                service.Dispose();

                btn.Enabled = false;

                //父控件
                var parent = btn.Parent;

                //找到启用按钮并启用交互
                var btnStart = Find<Button>(parent, _serTstartbtn, serviceName);
                btnStart.Enabled = true;

                //找到状态Label，并更新状态为：已停止
                var lbStatus = Find<Label>(parent, _serTstatus, serviceName);
                lbStatus.Text = "已停止";
                lbStatus.ForeColor = System.Drawing.Color.Red;

                //服务名称
                var servName = SysData.GetClearServiceName(serviceName);

                string message = string.Format("【{0}】 服务已停止！", servName);
                WriteMessage(message);

                //记录停止日志
                var loger = new ServerLoger(servName);
                loger.Write("服务已停止！");

            }
            #endregion
        }

        #endregion

        #region 队列服务

        /// <summary>
        /// 初始化队列服务
        /// </summary>
        private void InitQueueControls()
        {
            var list = SysData.QueueServiceList;

            //遍历服务类型
            for (var i = 0; i < list.Count; i++)
            {
                var serv = list[i];

                #region//服务项面板Panel
                Panel panel = new Panel();
                panel.Width = (int)Math.Floor(tabQueue.Width * 0.95);
                panel.Height = 30;
                var padLeft = (tabQueue.Width - panel.Width) / 2;
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
                startButton.Click += new EventHandler(BtnQueueStart_Click);
                panel.Controls.Add(startButton);
                #endregion

                #region//服务停止按钮
                Button stopButton = new Button();
                stopButton.Text = "停止";
                stopButton.Name = _serTstopbtn + serv.Name;
                stopButton.Tag = serv.Name;
                stopButton.Location = new System.Drawing.Point(400, 0);
                stopButton.Enabled = false;
                stopButton.Click += new EventHandler(BtnQueueStop_Click);
                panel.Controls.Add(stopButton);
                #endregion

                //#region//运行时长标题说明Lable
                //Label lbTimeTip = new Label();
                //lbTimeTip.Width = 70;
                //lbTimeTip.Text = "运行时长：";
                //lbTimeTip.Location = new System.Drawing.Point(500, 5);
                //panel.Controls.Add(lbTimeTip);
                //#endregion

                //#region//运行时长展示Label
                //Label lbTime = new Label();
                //lbTime.Width = 200;
                //lbTime.Text = "0天 0时 0分 0秒";
                //lbTime.Name = _serTtime + serv.Name;
                //lbTime.Location = new System.Drawing.Point(570, 5);
                //panel.Controls.Add(lbTime);
                //#endregion

                this.tabQueue.Controls.Add(panel);
            }
        }

        /// <summary>
        /// 队列服务启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQueueStart_Click(object sender, EventArgs e)
        {
            #region 获取当前正在操作的服务类型

            var btn = (Button)sender;

            var serviceName = (btn.Tag ?? string.Empty).ToString();

            #endregion

            #region 启动服务

            //当前服务的计划类型
            QueueScheduleType schedule = (QueueScheduleType)Enum.Parse(typeof(QueueScheduleType), serviceName);

            //任务计划服务
            SchedulerService service = null;

            switch (schedule)
            {
                //福利开奖 
                case QueueScheduleType.WelfareLottery:
                    service = new LotteryService(this, writeDelegate);
                    break;
                //福利报名提醒
                case QueueScheduleType.WelfareBaoMinRemind:
                    service = new WelfareBaoMinRemindService(this, writeDelegate);
                    break;
                //社区活动提醒
                case QueueScheduleType.CircleEventRemind:
                    service = new EventRemindService(this, writeDelegate);
                    break;
                //精品汇订单超时未支付
                case QueueScheduleType.MallOrderLatePayment:
                    service = new MallOrderLatePaymentService(this, writeDelegate);
                    break;
                //精品汇订单超时未收货
                case QueueScheduleType.MallOrderLateReceive:
                    service = new MallOrderLateReceiveService(this, writeDelegate);
                    break;
                //附近购订单超时未支付
                case QueueScheduleType.MerchantOrderLatePayment:
                    service = new MerchantOrderLatePaymentService(this, writeDelegate);
                    break;
                //附近购订单超时未收货
                case QueueScheduleType.MerchantOrderLateReceive:
                    service = new MerchantOrderLateReceiveService(this, writeDelegate);
                    break;
                //上门订单超时未支付
                case QueueScheduleType.VisitingOrderLatePayment:
                    service = new VisitingOrderLatePaymentService(this, writeDelegate);
                    break;
                //上门订单超时未确认服务完成
                case QueueScheduleType.VisitingOrderLateConfirmDone:
                    service = new VisitingOrderLateConfirmDoneService(this, writeDelegate);
                    break;
                //预约订单超时未支付
                case QueueScheduleType.ReservationOrderLatePayment:
                    service = new ReservationOrderLatePaymentService(this, writeDelegate);
                    break;
                //预约订单超时未确认服务完成
                case QueueScheduleType.ReservationOrderLateConfirmDone:
                    service = new ReservationOrderLateConfirmDoneService(this, writeDelegate);
                    break;
            }

            #endregion

            #region 

            if (service != null)
            {
                service.OnStart();

                _serviceCollection.Add(serviceName, service);

                //禁用启动按钮
                btn.Enabled = false;

                //父控件
                var parent = btn.Parent;

                //找到停止按钮并启用交互
                var btnStop = Find<Button>(parent, _serTstopbtn, serviceName);
                btnStop.Enabled = true;

                //找到状态Label，并更新状态为：运行中
                var lbStatus = Find<Label>(parent, _serTstatus, serviceName);
                lbStatus.Text = "运行中";
                lbStatus.ForeColor = System.Drawing.Color.Green;

                //服务已启动
                var servName = SysData.GetQueueServiceName(serviceName);

                ////找到运行时间显示控件
                //var lbTime = Find<Label>(parent, _serTtime, scheduleTypeName);

                ////服务计时
                //var clocker = new Clocker(scheduleTypeName, (obj) =>
                //{
                //    DateTime startTime = DateTime.Parse(obj.ToString());

                //    var timespan = DateTime.Now - startTime;

                //    var strTime = string.Format("{0}天 {1}小时 {2}分 {3}秒", timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds);

                //    this.Invoke((EventHandler)delegate
                //    {
                //        lbTime.Text = strTime;
                //    });
                //});

                //ClockerManager.Instance.Add(clocker);

                string message = string.Format("【{0}】 服务已启动！", servName);
                WriteMessage(message);

                //记录启动日志
                var loger = new ServerLoger(servName);
                loger.Write("服务已启动！");
            }
            #endregion
        }

        /// <summary>
        /// 队列服务停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQueueStop_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            var serviceName = (btn.Tag ?? string.Empty).ToString();

            #region 停止服务

            //任务计划服务
            SchedulerService service = _serviceCollection[serviceName];

            if (null != service)
            {
                service.Dispose();

                btn.Enabled = false;

                //父控件
                var parent = btn.Parent;

                //找到启用按钮并启用交互
                var btnStart = Find<Button>(parent, _serTstartbtn, serviceName);
                btnStart.Enabled = true;

                //找到状态Label，并更新状态为：已停止
                var lbStatus = Find<Label>(parent, _serTstatus, serviceName);
                lbStatus.Text = "已停止";
                lbStatus.ForeColor = System.Drawing.Color.Red;

                //服务名称
                var servName = SysData.GetQueueServiceName(serviceName);

                //停止计时器
                //ClockerManager.Instance.Stop(scheduleTypeName);

                string message = string.Format("【{0}】 服务已停止！", servName);
                WriteMessage(message);

                //记录停止日志
                var loger = new ServerLoger(servName);
                loger.Write("服务已停止！");
            }

            #endregion
        }

        #endregion

        #region 缓存维护  

        private void InitCacheControls()
        {
            int itemHight = 30;

            int marginTop = 10;

            #region 缓存更新周期设置

            Type enumLevelType = typeof(CacheLevel);

            //缓存级别列表
            var levelList = SysData.CacheLevelList;

            //定义外层Panel
            Panel periodSerlPanel = new Panel();
            periodSerlPanel.Width = (int)Math.Floor(tabCache.Width - 40M);
            var periodSerlPanelPadLeft = 10;
            periodSerlPanel.Location = new System.Drawing.Point(periodSerlPanelPadLeft, 10);
            periodSerlPanel.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);

            //外层Panel高度
            int levelPanelHight = 0;

            if (null != levelList)
            {
                for (var i = 0; i < levelList.Count; i++)
                {
                    var level = levelList[i];

                    //缓存更新周期
                    int time = 0;

                    //缓存更新周期时间单位
                    var timeOption = default(CacheTimeOption);

                    switch (level.EnumItem)
                    {
                        case CacheLevel.Hight: level.Description = "高"; time = 7; timeOption = CacheTimeOption.Day; break;
                        case CacheLevel.Lower: level.Description = "低"; time = 30; timeOption = CacheTimeOption.Minute; break;
                        case CacheLevel.Middel: level.Description = "中等"; time = 1; timeOption = CacheTimeOption.Day; break;
                        case CacheLevel.Permanent: level.Description = "持久"; time = 180; timeOption = CacheTimeOption.Day; break;
                    }

                    #region//缓存级别配置项面板 Panel
                    Panel panel = new Panel();
                    panel.Width = (int)Math.Floor(periodSerlPanel.Width * 0.95);
                    panel.Height = itemHight;
                    var padLeft = (periodSerlPanel.Width - panel.Width) / 2;
                    panel.Location = new System.Drawing.Point(padLeft, i * itemHight + marginTop);
                    #endregion

                    #region//缓存级别说明 Lable
                    Label lbType = new Label();
                    lbType.Width = 180;
                    lbType.Text = level.Description + "级别缓存更新时间周期为：";
                    lbType.Name = "lb_lv_" + level.Name;
                    lbType.Location = new System.Drawing.Point(0, 5);
                    panel.Controls.Add(lbType);
                    #endregion

                    #region //更新周期时间 TextBox
                    TextBox tbPeriod = new TextBox();
                    tbPeriod.Width = 100;
                    tbPeriod.Text = time.ToString();
                    tbPeriod.Name = "tb_lv_" + level.Name;
                    tbPeriod.Location = new System.Drawing.Point(180, 5);
                    panel.Controls.Add(tbPeriod);
                    #endregion

                    #region //更新周期时间单位 ComboBox
                    ComboBox combTimeOption = new ComboBox();
                    combTimeOption.Width = 50;
                    combTimeOption.Name = "comb_lv_" + level.Name;
                    combTimeOption.Location = new System.Drawing.Point(290, 5);
                    //绑定项
                    foreach (var to in SysData.CacheTimeOptionList)
                    {
                        combTimeOption.Items.Add(to.Description);

                        if (to.Value == (int)timeOption) combTimeOption.SelectedItem = to.Description;
                    }

                    panel.Controls.Add(combTimeOption);
                    #endregion

                    periodSerlPanel.Controls.Add(panel);
                }
            }

            #endregion

            #region 缓存维护服务控件绑定

            #region//服务项面板Panel
            Panel serPanel = new Panel();
            serPanel.Width = (int)Math.Floor(periodSerlPanel.Width * 0.95);
            serPanel.Height = itemHight;
            var ser_padLeft = (tabCache.Width - serPanel.Width) / 2;
            var ser_padTop = itemHight * levelList.Count + 30;
            serPanel.Location = new System.Drawing.Point(ser_padLeft, ser_padTop);
            #endregion

            //给定义外层Panel高度的变量赋值
            levelPanelHight = ser_padTop + serPanel.Height + 20;

            #region//服务类型名称Lable
            Label ser_lbType = new Label();
            ser_lbType.Width = 180;
            ser_lbType.Text = "小地方缓存维护服务";
            ser_lbType.Name = "cacheService";
            ser_lbType.Location = new System.Drawing.Point(0, 5);
            serPanel.Controls.Add(ser_lbType);
            #endregion

            #region//服务运行状态Label
            Label ser_lbStatus = new Label();
            ser_lbStatus.Width = 100;
            ser_lbStatus.Text = "未运行";
            ser_lbStatus.Name = "cacheServiceStatus";
            ser_lbStatus.Location = new System.Drawing.Point(180, 5);
            serPanel.Controls.Add(ser_lbStatus);
            #endregion

            #region//服务启动按钮
            Button ser_startButton = new Button();
            ser_startButton.Text = "启动";
            ser_startButton.Name = "btnCacheStart";
            ser_startButton.Tag = "cache";
            ser_startButton.Location = new System.Drawing.Point(300, 0);
            ser_startButton.Click += new EventHandler(BtnCacheStart_Click);
            serPanel.Controls.Add(ser_startButton);
            #endregion

            #region//服务停止按钮
            Button ser_stopButton = new Button();
            ser_stopButton.Text = "停止";
            ser_stopButton.Name = "btnCacheStop";
            ser_stopButton.Tag = "cache";
            ser_stopButton.Location = new System.Drawing.Point(400, 0);
            ser_stopButton.Enabled = false;
            ser_stopButton.Click += new EventHandler(BtnCacheStop_Click);
            serPanel.Controls.Add(ser_stopButton);
            #endregion

            periodSerlPanel.Controls.Add(serPanel);

            periodSerlPanel.Height = levelPanelHight;
            this.tabCache.Controls.Add(periodSerlPanel);

            #endregion

            #region 缓存级别设置与手动更新

            var cacheList = CacheCollection.GetAllCache();

            if (null != cacheList)
            {
                //外层Panel
                Panel levelSetPanel = new Panel();
                levelSetPanel.Width = (int)Math.Floor(tabCache.Width - 40M);
                levelSetPanel.Height = itemHight * cacheList.Count + 20;
                var levelSetPanelPadLeft = 10;
                levelSetPanel.Location = new System.Drawing.Point(levelSetPanelPadLeft, periodSerlPanel.Height + 20);
                levelSetPanel.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);

                int i = 0;
                foreach (var cache in cacheList)
                {
                    #region//缓存级别配置项面板 Panel
                    Panel panel = new Panel();
                    panel.Width = (int)Math.Floor(levelSetPanel.Width * 0.95);
                    panel.Height = itemHight;
                    var padLeft = (levelSetPanel.Width - panel.Width) / 2;
                    panel.Location = new System.Drawing.Point(padLeft, i++ * itemHight + marginTop);
                    #endregion

                    #region//缓存名称 Lable
                    Label lbType = new Label();
                    lbType.Width = 200;
                    lbType.Text = Td.Common.EnumUtility.GetEnumDescription<CacheItemType>(cache.ItemType.ToString());// cache.CacheKey;
                    lbType.Location = new System.Drawing.Point(0, 5);
                    panel.Controls.Add(lbType);
                    #endregion

                    #region//等级 Lable
                    Label lbLevel = new Label();
                    lbLevel.Width = 45;
                    lbLevel.Text = "级别：";
                    lbLevel.Location = new System.Drawing.Point(200, 5);
                    panel.Controls.Add(lbLevel);
                    #endregion

                    #region //级别 ComboBox
                    ComboBox combLevelSet = new ComboBox();
                    combLevelSet.Width = 50;
                    combLevelSet.Name = "comb_setlv_" + cache.CacheKey;
                    combLevelSet.Tag = cache.ItemType.ToString("d");
                    combLevelSet.Location = new System.Drawing.Point(250, 5);
                    //绑定项
                    foreach (var to in SysData.CacheLevelList)
                    {
                        combLevelSet.Items.Add(to.Description);

                        if (to.Value == (int)cache.Level) combLevelSet.SelectedItem = to.Description;
                    }
                    combLevelSet.SelectedIndexChanged += CombLeveSet_SelectedIndexChanged;
                    panel.Controls.Add(combLevelSet);
                    #endregion

                    #region //更新按钮 Button
                    Button btnUpdateCacheItem = new Button();
                    btnUpdateCacheItem.Text = "更新";
                    btnUpdateCacheItem.Name = "btnCacheUpdate_" + cache.CacheKey;
                    btnUpdateCacheItem.Tag = cache.ItemType.ToString("d");
                    btnUpdateCacheItem.Width = 60;
                    btnUpdateCacheItem.Location = new System.Drawing.Point(330, 0);
                    btnUpdateCacheItem.Click += new EventHandler(BtnUpdateCacheItem_Click);
                    panel.Controls.Add(btnUpdateCacheItem);
                    #endregion

                    #region //查看缓存数据

                    Button btnShowData = new Button();
                    btnShowData.Text = "查看";
                    btnShowData.Name = "btnshowdata_" + cache.CacheKey;
                    btnShowData.Tag = cache.ItemType.ToString("d");
                    btnShowData.Location = new System.Drawing.Point(405, 0);
                    btnShowData.Click += new EventHandler(BtnShowData_Click);
                    panel.Controls.Add(btnShowData);

                    #endregion

                    levelSetPanel.Controls.Add(panel);
                }

                this.tabCache.Controls.Add(levelSetPanel);
            }

            #endregion
        }

        /// <summary>
        /// 缓存更新服务开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCacheStart_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            #region 获取并更新配置

            List<CacheMaintainConfig> configs = new List<CacheMaintainConfig>();

            //缓存级别列表
            var levelList = typeof(CacheLevel).GetEnumDesc<CacheLevel>();

            foreach (var lv in levelList)
            {
                //获取级别对应的周期
                TextBox tbPeriod = Find<TextBox>(this.tabCache, "tb_lv_", lv.Name);
                int time = 0;
                if (null != tbPeriod)
                {
                    int.TryParse(tbPeriod.Text.Trim(), out time);
                }

                //获取级别对应的周期单位
                ComboBox combTimeOption = Find<ComboBox>(this.tabCache, "comb_lv_", lv.Name);
                CacheTimeOption option = CacheTimeOption.Day;
                if (null != combTimeOption)
                {
                    string strOption = combTimeOption.SelectedItem.ToString();

                    var _tempOption = SysData.CacheTimeOptionList.FirstOrDefault(p => p.Description.Equals(strOption));

                    if (null != _tempOption)
                    {
                        option = (CacheTimeOption)Enum.Parse(typeof(CacheTimeOption), _tempOption.Name);
                    }
                }

                configs.Add(new CacheMaintainConfig
                {
                    Level = (CacheLevel)Enum.Parse(typeof(CacheLevel), lv.Name),
                    PeriodTime = time,
                    TimeOption = option
                });
            }
            //更新缓存维护参数配置
            Startup.UpdateCacheMaintainConfig(configs);

            #endregion

            #region 启动服务

            string serviceName = "缓存维护服务";

            SchedulerService service = new CacheMaintainService(serviceName, this, writeDelegate);

            service.OnStart();

            _serviceCollection.Add(serviceName, service);

            //禁用启动按钮
            btn.Enabled = false;

            //找到停止按钮并启用交互
            var btnStop = Find<Button>(this.tabCache, "btnCacheStop");
            btnStop.Enabled = true;

            //找到状态Label，并更新状态为：运行中
            var lbStatus = Find<Label>(this.tabCache, "cacheServiceStatus");
            lbStatus.Text = "运行中";
            lbStatus.ForeColor = System.Drawing.Color.Green;

            string message = string.Format("【{0}】 服务已启动！", serviceName);
            WriteMessage(message);

            //记录启动日志
            var loger = new ServerLoger(serviceName);
            loger.Write("服务已启动！");

            #endregion
        }

        /// <summary>
        /// 缓存更新服务结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void BtnCacheStop_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            #region 停止服务

            //服务名称
            var serviceName = "缓存维护服务";

            //任务计划服务
            SchedulerService service = _serviceCollection[serviceName];

            if (null != service)
            {
                service.Dispose();

                btn.Enabled = false;

                //找到启用按钮并启用交互
                var btnStart = Find<Button>(this.tabCache, "btnCacheStart");
                btnStart.Enabled = true;

                //找到状态Label，并更新状态为：已停止
                var lbStatus = Find<Label>(this.tabCache, "cacheServiceStatus");
                lbStatus.Text = "已停止";
                lbStatus.ForeColor = System.Drawing.Color.Red;


                string message = string.Format("【{0}】 服务已停止！", serviceName);
                WriteMessage(message);

                //记录停止日志
                var loger = new ServerLoger(serviceName);
                loger.Write("服务已停止！");
            }

            #endregion
        }

        /// <summary>
        /// 设置缓存级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CombLeveSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (null == combo) return;

            string tag = combo.Tag.ToString();

            string selValue = combo.SelectedItem.ToString();

            CacheItemType itemType = (CacheItemType)Enum.Parse(typeof(CacheItemType), tag);

            //缓存级别列表
            var levelList = SysData.CacheLevelList;

            var level = SysData.CacheLevelList.FirstOrDefault(p => p.Description.ToString() == selValue);

            var cache = CacheCollection.GetCache(itemType);

            if (null != cache)
            {
                cache.ResetLevel(level.EnumItem);

                WriteMessage(string.Format("{0}缓存级别调整为“{1}”。", cache.CacheKey, level.Description));
            }
        }

        /// <summary>
        /// 更新缓存项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void BtnUpdateCacheItem_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (null == btn) return;

            string tag = btn.Tag.ToString();

            CacheItemType itemType = (CacheItemType)Enum.Parse(typeof(CacheItemType), tag);

            var cache = CacheCollection.GetCache(itemType);

            if (null != cache)
            {
                cache.Update();

                var cacheName = Td.Common.EnumUtility.GetEnumDescription<CacheItemType>(cache.ItemType.ToString());

                WriteMessage(string.Format("缓存“{0}”已更新！", cacheName));

                var data = cache.GetCacheData();

                StringBuilder sb = new StringBuilder();
                
                sb.AppendLine(string.Format("缓存{0}更新后的源数据为：", cacheName));

                if (null != data)
                {
                    foreach (var item in data)
                    {
                        string temp = (item ?? string.Empty).ToString();
                        sb.AppendLine(temp);
                    }
                }

                WriteMessage(sb.ToString());
            }
        }

        /// <summary>
        /// 显示缓存数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShowData_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (null == btn) return;

            string tag = btn.Tag.ToString();

            CacheItemType itemType = (CacheItemType)Enum.Parse(typeof(CacheItemType), tag);

            var cache = CacheCollection.GetCache(itemType);

            var db = CacheCollection.MyRedisDB();

            if (null != cache)
            {
                var data = cache.GetCacheData();
                
                StringBuilder sb = new StringBuilder();

                var cacheName = Td.Common.EnumUtility.GetEnumDescription<CacheItemType>(cache.ItemType.ToString());

                sb.AppendLine(string.Format("缓存{0}的源数据为：",cacheName));

                if (null != data)
                {
                    foreach (var item in data)
                    {
                        string temp = (item??string.Empty).ToString();
                        sb.AppendLine(temp);
                    }
                }

                WriteMessage(sb.ToString());
            }
        }

        #endregion

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
            string name = tagName + scheduleType;

            return Find<T>(parent, name);
        }

        /// <summary>
        /// 获取控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private T Find<T>(Control parent, string name) where T : Control
        {
            var find = Fix<Control, string, T>(f => (control, controlName) =>
            {
                T rstControl = null;

                Type type = typeof(T);

                var controls = control.Controls;

                foreach (Control item in controls)
                {
                    if (item.Name == controlName)
                    {
                        rstControl = (T)item;
                        break;
                    }
                    else
                    {
                        rstControl = f(item, controlName);
                        if (rstControl != null)
                        {
                            break;
                        }
                    }
                }

                return rstControl;
            });

            return find(parent, name);
        }

        /// <summary>
        /// 不动点算子函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="g"></param>
        /// <returns></returns>
        Func<T1, T2, TResult> Fix<T1, T2, TResult>(Func<Func<T1, T2, TResult>, Func<T1, T2, TResult>> g)
        {
            return (x, y) => g(Fix(g))(x, y);
        }

        #endregion

        #region////////////////// 结果/消息输出/////////////////////

        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="message"></param>
        private void WriteMessage(string message, bool padTime = true)
        {
            if (padTime)
            {
                message += DateTime.Now.ToString("      ### yyyy-MM-dd HH:mm:ss ###");
            }
            this.richMessage.AppendText(message);
            this.richMessage.AppendText("\n");
            this.richMessage.Focus();
        }

        #endregion

        #region /////////////////查看/更新配置/////////////////////////

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateConfig_Click(object sender, EventArgs e)
        {
            UpdateForm form = new UpdateForm();
            form.Show();
        }

        /// <summary>
        /// 查看配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLookConfig_Click(object sender, EventArgs e)
        {
            var configForm = new ConfigForm();
            configForm.Show();
        }

        #endregion

        /// <summary>
        /// 绑定产品信息及维护人员信息
        /// </summary>
        void InitProductInfo()
        {
            this.Text = string.Format("{0}（v{1}）{2} / QQ:{3} / {4} / {5}",
                Startup.ProductInfo.ProductName,
                Startup.ProductInfo.Version,
                Startup.ProductInfo.Author,
                Startup.ProductInfo.QQ,
                Startup.ProductInfo.Email,
                Startup.ProductInfo.Mobile);
        }

        /// <summary>
        /// 默认显示
        /// </summary>
        void InitShow()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("┏＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝┓");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　★★★★★★★★★★★★★★★★★★★★　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　  感谢默默坚守在服务后台的机器人　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　★★★★★★★★★★★★★★★★★★★★　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　           →谨以此诗表达团队人员对你的敬佩之情！　　 　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　来一首极度华丽的诗词：　　　      　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　红 酥 手   黄 滕 酒   两 个 黄 鹂 鸣 翠 柳   　　　　 　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　 ~~长 亭 外　 古 道 边  一 行 白 鹭 上 青 天 ~~~　　　  　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("╃　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　┾");
            sb.AppendLine("┗＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝┛");

            WriteMessage(sb.ToString(), false);
        }

        #region //关闭与托盘

        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;

            if (null == notifyIcon)
            {
                InitialTray();
            }
        }

        /// <summary>
        /// 初始化托盘
        /// </summary>
        private void InitialTray()
        {
            //实例化一个NotifyIcon对象  
            notifyIcon = new NotifyIcon();
            //托盘图标气泡显示的内容  
            notifyIcon.BalloonTipText = string.Format("{0}正在后台运行", Startup.ProductInfo.ProductName);
            //托盘图标显示的内容  
            notifyIcon.Text = Startup.ProductInfo.ProductName;
            string icon = AppDomain.CurrentDomain.BaseDirectory + "/kylin.ico";
            if (File.Exists(icon))
            {
                //注意：下面的路径可以是绝对路径、相对路径。但是需要注意的是：文件必须是一个.ico格式  
                notifyIcon.Icon = new System.Drawing.Icon(icon);
            }
            //true表示在托盘区可见，false表示在托盘区不可见  
            notifyIcon.Visible = true;
            //气泡显示的时间（单位是毫秒）  
            notifyIcon.ShowBalloonTip(1000);
            notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_MouseClick);

            //退出菜单项  
            MenuItem exit = new MenuItem("退出");
            exit.Click += new EventHandler(exit_Click);

            //关联托盘控件
            MenuItem[] childen = new MenuItem[] { exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);
        }

        /// <summary>  
        /// 鼠标单击  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //鼠标左键单击  
            if (e.Button == MouseButtons.Left)
            {
                //如果窗体是可见的，那么鼠标左击托盘区图标后，窗体为不可见  
                if (this.Visible == true)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = true;
                    this.Activate();
                }
            }
        }

        /// <summary>  
        /// 退出选项  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void exit_Click(object sender, EventArgs e)
        {
            //退出程序  
            Environment.Exit(0);
        }

        #endregion
    }
}

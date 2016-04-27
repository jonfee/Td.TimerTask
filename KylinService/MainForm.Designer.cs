using System.Windows.Forms;

namespace KylinService
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.richMessage = new System.Windows.Forms.RichTextBox();
            this.groupTool = new System.Windows.Forms.GroupBox();
            this.btnLookConfig = new System.Windows.Forms.Button();
            this.btnUpdateConfig = new System.Windows.Forms.Button();
            this.tabMenu = new System.Windows.Forms.TabControl();
            this.tabClear = new System.Windows.Forms.TabPage();
            this.tabCache = new System.Windows.Forms.TabPage();
            this.tabQueue = new System.Windows.Forms.TabPage();
            this.groupTool.SuspendLayout();
            this.tabMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // richMessage
            // 
            this.richMessage.BackColor = System.Drawing.SystemColors.Control;
            this.richMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richMessage.Location = new System.Drawing.Point(598, 20);
            this.richMessage.Name = "richMessage";
            this.richMessage.Size = new System.Drawing.Size(644, 664);
            this.richMessage.TabIndex = 1;
            this.richMessage.Text = "";
            // 
            // groupTool
            // 
            this.groupTool.Controls.Add(this.btnLookConfig);
            this.groupTool.Controls.Add(this.btnUpdateConfig);
            this.groupTool.Location = new System.Drawing.Point(13, 13);
            this.groupTool.Name = "groupTool";
            this.groupTool.Size = new System.Drawing.Size(568, 61);
            this.groupTool.TabIndex = 2;
            this.groupTool.TabStop = false;
            this.groupTool.Text = "系统参数更新";
            // 
            // btnLookConfig
            // 
            this.btnLookConfig.Location = new System.Drawing.Point(384, 21);
            this.btnLookConfig.Name = "btnLookConfig";
            this.btnLookConfig.Size = new System.Drawing.Size(75, 23);
            this.btnLookConfig.TabIndex = 1;
            this.btnLookConfig.Text = "查看配置";
            this.btnLookConfig.UseVisualStyleBackColor = true;
            this.btnLookConfig.Click += new System.EventHandler(this.btnLookConfig_Click);
            // 
            // btnUpdateConfig
            // 
            this.btnUpdateConfig.Location = new System.Drawing.Point(476, 21);
            this.btnUpdateConfig.Name = "btnUpdateConfig";
            this.btnUpdateConfig.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateConfig.TabIndex = 0;
            this.btnUpdateConfig.Text = "更新配置";
            this.btnUpdateConfig.UseVisualStyleBackColor = true;
            this.btnUpdateConfig.Click += new System.EventHandler(this.btnUpdateConfig_Click);
            // 
            // tabMenu
            // 
            this.tabMenu.Controls.Add(this.tabClear);
            this.tabMenu.Controls.Add(this.tabCache);
            this.tabMenu.Controls.Add(this.tabQueue);
            this.tabMenu.Location = new System.Drawing.Point(13, 90);
            this.tabMenu.Name = "tabMenu";
            this.tabMenu.SelectedIndex = 0;
            this.tabMenu.Size = new System.Drawing.Size(568, 594);
            this.tabMenu.TabIndex = 3;
            // 
            // tabClear
            // 
            this.tabClear.Location = new System.Drawing.Point(4, 22);
            this.tabClear.Name = "tabClear";
            this.tabClear.Size = new System.Drawing.Size(560, 568);
            this.tabClear.TabIndex = 2;
            this.tabClear.Text = "定期清理";
            this.tabClear.UseVisualStyleBackColor = true;
            // 
            // tabCache
            // 
            this.tabCache.Location = new System.Drawing.Point(4, 22);
            this.tabCache.Name = "tabCache";
            this.tabCache.Padding = new System.Windows.Forms.Padding(3);
            this.tabCache.Size = new System.Drawing.Size(560, 568);
            this.tabCache.TabIndex = 1;
            this.tabCache.Text = "缓存维护";
            this.tabCache.UseVisualStyleBackColor = true;
            // 
            // tabQueue
            // 
            this.tabQueue.Location = new System.Drawing.Point(4, 22);
            this.tabQueue.Name = "tabQueue";
            this.tabQueue.Padding = new System.Windows.Forms.Padding(3);
            this.tabQueue.Size = new System.Drawing.Size(560, 568);
            this.tabQueue.TabIndex = 0;
            this.tabQueue.Text = "队列服务";
            this.tabQueue.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1254, 696);
            this.Controls.Add(this.tabMenu);
            this.Controls.Add(this.groupTool);
            this.Controls.Add(this.richMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Kylin智能服务托管";
            this.groupTool.ResumeLayout(false);
            this.tabMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);

        }

        #endregion
        private System.Windows.Forms.RichTextBox richMessage;
        private System.Windows.Forms.GroupBox groupTool;
        private System.Windows.Forms.Button btnUpdateConfig;
        private System.Windows.Forms.Button btnLookConfig;
        private System.Windows.Forms.TabControl tabMenu;
        private System.Windows.Forms.TabPage tabQueue;
        private System.Windows.Forms.TabPage tabCache;
        private System.Windows.Forms.TabPage tabClear;
    }
}


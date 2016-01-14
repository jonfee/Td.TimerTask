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
            this.groupSchedule = new System.Windows.Forms.GroupBox();
            this.richMessage = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // groupSchedule
            // 
            this.groupSchedule.Location = new System.Drawing.Point(13, 13);
            this.groupSchedule.Name = "groupSchedule";
            this.groupSchedule.Size = new System.Drawing.Size(795, 168);
            this.groupSchedule.TabIndex = 0;
            this.groupSchedule.TabStop = false;
            this.groupSchedule.Text = "定时任务设置";
            // 
            // richMessage
            // 
            this.richMessage.Location = new System.Drawing.Point(13, 199);
            this.richMessage.Name = "richMessage";
            this.richMessage.Size = new System.Drawing.Size(795, 289);
            this.richMessage.TabIndex = 1;
            this.richMessage.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 500);
            this.Controls.Add(this.richMessage);
            this.Controls.Add(this.groupSchedule);
            this.Name = "MainForm";
            this.Text = "Kylin智能服务托管";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupSchedule;
        private System.Windows.Forms.RichTextBox richMessage;
    }
}


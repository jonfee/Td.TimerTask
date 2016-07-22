namespace KylinService
{
    partial class ExceptionSetForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ckbDisplay = new System.Windows.Forms.CheckBox();
            this.ckbWriteFile = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ckbDisplay
            // 
            this.ckbDisplay.AutoSize = true;
            this.ckbDisplay.Location = new System.Drawing.Point(57, 33);
            this.ckbDisplay.Name = "ckbDisplay";
            this.ckbDisplay.Size = new System.Drawing.Size(108, 16);
            this.ckbDisplay.TabIndex = 0;
            this.ckbDisplay.Text = "输出到消息面板";
            this.ckbDisplay.UseVisualStyleBackColor = true;
            this.ckbDisplay.CheckedChanged += new System.EventHandler(ckbDisplay_CheckedChanged);
            // 
            // ckbWriteFile
            // 
            this.ckbWriteFile.AutoSize = true;
            this.ckbWriteFile.Location = new System.Drawing.Point(57, 65);
            this.ckbWriteFile.Name = "ckbWriteFile";
            this.ckbWriteFile.Size = new System.Drawing.Size(108, 16);
            this.ckbWriteFile.TabIndex = 1;
            this.ckbWriteFile.Text = "写入到日志文件";
            this.ckbWriteFile.UseVisualStyleBackColor = true;
            this.ckbWriteFile.CheckedChanged += new System.EventHandler(ckbWriteFile_CheckedChanged);
            // 
            // ExceptionSetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(224, 120);
            this.Controls.Add(this.ckbWriteFile);
            this.Controls.Add(this.ckbDisplay);
            this.Name = "ExceptionSetForm";
            this.Text = "异常处理设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ckbDisplay;
        private System.Windows.Forms.CheckBox ckbWriteFile;
    }
}
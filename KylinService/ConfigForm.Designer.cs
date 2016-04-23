namespace KylinService
{
    partial class ConfigForm
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
            this.rtContent = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtContent
            // 
            this.rtContent.BackColor = System.Drawing.SystemColors.Control;
            this.rtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtContent.Location = new System.Drawing.Point(13, 13);
            this.rtContent.Name = "rtContent";
            this.rtContent.Size = new System.Drawing.Size(812, 522);
            this.rtContent.TabIndex = 0;
            this.rtContent.Text = "";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(837, 538);
            this.Controls.Add(this.rtContent);
            this.Name = "ConfigForm";
            this.Text = "系统参数配置详情";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtContent;
    }
}
namespace KylinService
{
    partial class UpdateForm
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
            this.tbSqlConn = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSqlUpdate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbCacheRedisConn = new System.Windows.Forms.TextBox();
            this.btnCacheRedisUpdate = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbScheduleRedisConn = new System.Windows.Forms.TextBox();
            this.btnScheduleRedisUpdate = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPushRedisConn = new System.Windows.Forms.TextBox();
            this.btnPushRedisUpdate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnAppointOrderConfigUpdate = new System.Windows.Forms.Button();
            this.btnCircleEventConfigUpdate = new System.Windows.Forms.Button();
            this.btnMerchantOrderConfigUpdate = new System.Windows.Forms.Button();
            this.btnLegworkConfigUpdate = new System.Windows.Forms.Button();
            this.btnWelfareConfigUpdate = new System.Windows.Forms.Button();
            this.btnB2COrderConfigUpdate = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.comboSqlType = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbSqlConn
            // 
            this.tbSqlConn.Location = new System.Drawing.Point(134, 74);
            this.tbSqlConn.Name = "tbSqlConn";
            this.tbSqlConn.Size = new System.Drawing.Size(373, 21);
            this.tbSqlConn.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "数据库连接：";
            // 
            // btnSqlUpdate
            // 
            this.btnSqlUpdate.Location = new System.Drawing.Point(560, 74);
            this.btnSqlUpdate.Name = "btnSqlUpdate";
            this.btnSqlUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnSqlUpdate.TabIndex = 2;
            this.btnSqlUpdate.Text = "更新";
            this.btnSqlUpdate.UseVisualStyleBackColor = true;
            this.btnSqlUpdate.Click += new System.EventHandler(this.btnSqlUpdate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "数据缓存Redis连接：";
            // 
            // tbCacheRedisConn
            // 
            this.tbCacheRedisConn.Location = new System.Drawing.Point(172, 34);
            this.tbCacheRedisConn.Name = "tbCacheRedisConn";
            this.tbCacheRedisConn.Size = new System.Drawing.Size(373, 21);
            this.tbCacheRedisConn.TabIndex = 4;
            // 
            // btnCacheRedisUpdate
            // 
            this.btnCacheRedisUpdate.Location = new System.Drawing.Point(564, 34);
            this.btnCacheRedisUpdate.Name = "btnCacheRedisUpdate";
            this.btnCacheRedisUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnCacheRedisUpdate.TabIndex = 5;
            this.btnCacheRedisUpdate.Text = "更新";
            this.btnCacheRedisUpdate.UseVisualStyleBackColor = true;
            this.btnCacheRedisUpdate.Click += new System.EventHandler(this.btnCacheRedisUpdate_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "任务计划队列Redis连接：";
            // 
            // tbScheduleRedisConn
            // 
            this.tbScheduleRedisConn.Location = new System.Drawing.Point(172, 81);
            this.tbScheduleRedisConn.Name = "tbScheduleRedisConn";
            this.tbScheduleRedisConn.Size = new System.Drawing.Size(373, 21);
            this.tbScheduleRedisConn.TabIndex = 7;
            // 
            // btnScheduleRedisUpdate
            // 
            this.btnScheduleRedisUpdate.Location = new System.Drawing.Point(564, 79);
            this.btnScheduleRedisUpdate.Name = "btnScheduleRedisUpdate";
            this.btnScheduleRedisUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnScheduleRedisUpdate.TabIndex = 8;
            this.btnScheduleRedisUpdate.Text = "更新";
            this.btnScheduleRedisUpdate.UseVisualStyleBackColor = true;
            this.btnScheduleRedisUpdate.Click += new System.EventHandler(this.btnScheduleRedisUpdate_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "消息推送队列Redis连接：";
            // 
            // tbPushRedisConn
            // 
            this.tbPushRedisConn.Location = new System.Drawing.Point(172, 126);
            this.tbPushRedisConn.Name = "tbPushRedisConn";
            this.tbPushRedisConn.Size = new System.Drawing.Size(373, 21);
            this.tbPushRedisConn.TabIndex = 10;
            // 
            // btnPushRedisUpdate
            // 
            this.btnPushRedisUpdate.Location = new System.Drawing.Point(564, 124);
            this.btnPushRedisUpdate.Name = "btnPushRedisUpdate";
            this.btnPushRedisUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnPushRedisUpdate.TabIndex = 11;
            this.btnPushRedisUpdate.Text = "更新";
            this.btnPushRedisUpdate.UseVisualStyleBackColor = true;
            this.btnPushRedisUpdate.Click += new System.EventHandler(this.btnPushRedisUpdate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnPushRedisUpdate);
            this.groupBox1.Controls.Add(this.tbPushRedisConn);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnScheduleRedisUpdate);
            this.groupBox1.Controls.Add(this.tbCacheRedisConn);
            this.groupBox1.Controls.Add(this.tbScheduleRedisConn);
            this.groupBox1.Controls.Add(this.btnCacheRedisUpdate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(15, 164);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(655, 174);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Redis服务器参数";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnAppointOrderConfigUpdate);
            this.groupBox2.Controls.Add(this.btnCircleEventConfigUpdate);
            this.groupBox2.Controls.Add(this.btnMerchantOrderConfigUpdate);
            this.groupBox2.Controls.Add(this.btnLegworkConfigUpdate);
            this.groupBox2.Controls.Add(this.btnWelfareConfigUpdate);
            this.groupBox2.Controls.Add(this.btnB2COrderConfigUpdate);
            this.groupBox2.Location = new System.Drawing.Point(15, 371);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(657, 170);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "其它";
            // 
            // btnAppointOrderConfigUpdate
            // 
            this.btnAppointOrderConfigUpdate.Location = new System.Drawing.Point(356, 125);
            this.btnAppointOrderConfigUpdate.Name = "btnAppointOrderConfigUpdate";
            this.btnAppointOrderConfigUpdate.Size = new System.Drawing.Size(278, 23);
            this.btnAppointOrderConfigUpdate.TabIndex = 1;
            this.btnAppointOrderConfigUpdate.Text = "上门预约订单处理配置 [更新]";
            this.btnAppointOrderConfigUpdate.UseVisualStyleBackColor = true;
            this.btnAppointOrderConfigUpdate.Click += new System.EventHandler(this.btnAppointOrderConfigUpdate_Click);
            // 
            // btnCircleEventConfigUpdate
            // 
            this.btnCircleEventConfigUpdate.Location = new System.Drawing.Point(356, 80);
            this.btnCircleEventConfigUpdate.Name = "btnCircleEventConfigUpdate";
            this.btnCircleEventConfigUpdate.Size = new System.Drawing.Size(278, 23);
            this.btnCircleEventConfigUpdate.TabIndex = 1;
            this.btnCircleEventConfigUpdate.Text = "社区活动提醒配置 [更新]";
            this.btnCircleEventConfigUpdate.UseVisualStyleBackColor = true;
            this.btnCircleEventConfigUpdate.Click += new System.EventHandler(this.btnCircleEventConfigUpdate_Click);
            // 
            // btnMerchantOrderConfigUpdate
            // 
            this.btnMerchantOrderConfigUpdate.Location = new System.Drawing.Point(356, 34);
            this.btnMerchantOrderConfigUpdate.Name = "btnMerchantOrderConfigUpdate";
            this.btnMerchantOrderConfigUpdate.Size = new System.Drawing.Size(278, 23);
            this.btnMerchantOrderConfigUpdate.TabIndex = 1;
            this.btnMerchantOrderConfigUpdate.Text = "附近购订单处理配置 [更新]";
            this.btnMerchantOrderConfigUpdate.UseVisualStyleBackColor = true;
            this.btnMerchantOrderConfigUpdate.Click += new System.EventHandler(this.btnMerchantOrderConfigUpdate_Click);
            // 
            // btnLegworkConfigUpdate
            // 
            this.btnLegworkConfigUpdate.Location = new System.Drawing.Point(22, 125);
            this.btnLegworkConfigUpdate.Name = "btnLegworkConfigUpdate";
            this.btnLegworkConfigUpdate.Size = new System.Drawing.Size(277, 23);
            this.btnLegworkConfigUpdate.TabIndex = 0;
            this.btnLegworkConfigUpdate.Text = "跑腿订单处理配置 [更新]";
            this.btnLegworkConfigUpdate.UseVisualStyleBackColor = true;
            this.btnLegworkConfigUpdate.Click += new System.EventHandler(this.btnLegworkConfigUpdate_Click);
            // 
            // btnWelfareConfigUpdate
            // 
            this.btnWelfareConfigUpdate.Location = new System.Drawing.Point(22, 80);
            this.btnWelfareConfigUpdate.Name = "btnWelfareConfigUpdate";
            this.btnWelfareConfigUpdate.Size = new System.Drawing.Size(277, 23);
            this.btnWelfareConfigUpdate.TabIndex = 0;
            this.btnWelfareConfigUpdate.Text = "限时福利业务处理配置 [更新]";
            this.btnWelfareConfigUpdate.UseVisualStyleBackColor = true;
            this.btnWelfareConfigUpdate.Click += new System.EventHandler(this.btnWelfareConfigUpdate_Click);
            // 
            // btnB2COrderConfigUpdate
            // 
            this.btnB2COrderConfigUpdate.Location = new System.Drawing.Point(22, 34);
            this.btnB2COrderConfigUpdate.Name = "btnB2COrderConfigUpdate";
            this.btnB2COrderConfigUpdate.Size = new System.Drawing.Size(277, 23);
            this.btnB2COrderConfigUpdate.TabIndex = 0;
            this.btnB2COrderConfigUpdate.Text = "精品汇订单处理配置 [更新]";
            this.btnB2COrderConfigUpdate.UseVisualStyleBackColor = true;
            this.btnB2COrderConfigUpdate.Click += new System.EventHandler(this.btnB2COrderConfigUpdate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(44, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "数据库类型：";
            // 
            // comboSqlType
            // 
            this.comboSqlType.FormattingEnabled = true;
            this.comboSqlType.Location = new System.Drawing.Point(134, 31);
            this.comboSqlType.Name = "comboSqlType";
            this.comboSqlType.Size = new System.Drawing.Size(121, 20);
            this.comboSqlType.TabIndex = 12;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboSqlType);
            this.groupBox3.Controls.Add(this.tbSqlConn);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.btnSqlUpdate);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(15, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(656, 124);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "数据库连接";
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 553);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "UpdateForm";
            this.Text = " ";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbSqlConn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSqlUpdate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbCacheRedisConn;
        private System.Windows.Forms.Button btnCacheRedisUpdate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbScheduleRedisConn;
        private System.Windows.Forms.Button btnScheduleRedisUpdate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPushRedisConn;
        private System.Windows.Forms.Button btnPushRedisUpdate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnB2COrderConfigUpdate;
        private System.Windows.Forms.Button btnMerchantOrderConfigUpdate;
        private System.Windows.Forms.Button btnWelfareConfigUpdate;
        private System.Windows.Forms.Button btnCircleEventConfigUpdate;
        private System.Windows.Forms.Button btnLegworkConfigUpdate;
        private System.Windows.Forms.Button btnAppointOrderConfigUpdate;
        private System.Windows.Forms.ComboBox comboSqlType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}
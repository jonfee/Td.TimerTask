using KylinService.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Td.Kylin.DataCache;
using Td.Kylin.EnumLibrary;

namespace KylinService
{
    public partial class UpdateForm : Form
    {
        public UpdateForm()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            var sqlTypeDic = typeof(SqlProviderType).GetEnumDesc<SqlProviderType>();
            foreach (var sql in sqlTypeDic)
            {
                this.comboSqlType.Items.Add(sql.Name);
                if (sql.Name.Equals(Startup.SqlType.ToString(), StringComparison.OrdinalIgnoreCase)) this.comboSqlType.SelectedItem = sql.Name;
            }

            this.tbSqlConn.Text = Startup.KylinDBConnectionString;
            this.tbCacheRedisConn.Text = Startup.DataCacheRedisConnectionString;
            this.tbScheduleRedisConn.Text = Startup.ScheduleRedisConfigs.Items.FirstOrDefault()?.ConnectionString;
            this.tbPushRedisConn.Text = Startup.PushRedisConfigs.Items.FirstOrDefault()?.ConnectionString;
        }

        private void btnSqlUpdate_Click(object sender, EventArgs e)
        {
            string sqlConn = this.tbSqlConn.Text.Trim();

            SqlProviderType sqlType = new Func<SqlProviderType>(() =>
              {
                  string sqltype = this.comboSqlType.SelectedText;

                  switch (sqltype.ToLower())
                  {
                      case "npgsql":
                          return SqlProviderType.NpgSQL;
                      case "mssql":
                      default:
                          return SqlProviderType.SqlServer;
                  }

              }).Invoke();

            Task.Run(() =>
            {
                try
                {
                    Startup.UpdateSqlConnection(sqlType, sqlConn);
                    MessageBox.Show("更新成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void btnCacheRedisUpdate_Click(object sender, EventArgs e)
        {
            string redisConn = this.tbCacheRedisConn.Text.Trim();

            Task.Run(() =>
            {
                try
                {
                    Startup.UpdateDataCacheRedis(redisConn);
                    MessageBox.Show("更新成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void btnScheduleRedisUpdate_Click(object sender, EventArgs e)
        {
            string redisConn = this.tbScheduleRedisConn.Text.Trim();
            Task.Run(() =>
            {
                try
                {
                    Startup.UpdateScheduleRedis(redisConn);
                    MessageBox.Show("更新成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void btnPushRedisUpdate_Click(object sender, EventArgs e)
        {
            string redisConn = this.tbPushRedisConn.Text.Trim();
            Task.Run(() =>
            {
                try
                {
                    Startup.UpdatePushRedis(redisConn);
                    MessageBox.Show("更新成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void btnB2COrderConfigUpdate_Click(object sender, EventArgs e)
        {
            Startup.UpdateB2COrderConfig();
            MessageBox.Show("更新成功");
        }

        private void btnMerchantOrderConfigUpdate_Click(object sender, EventArgs e)
        {
            Startup.UpdateMerchantOrderConfig();
            MessageBox.Show("更新成功");
        }

        private void btnWelfareConfigUpdate_Click(object sender, EventArgs e)
        {
            Startup.UpdateWelfareConfig();
            MessageBox.Show("更新成功");
        }

        private void btnCircleEventConfigUpdate_Click(object sender, EventArgs e)
        {
            Startup.UpdateCircleConfig();
            MessageBox.Show("更新成功");
        }

        private void btnAppointOrderConfigUpdate_Click(object sender, EventArgs e)
        {
            Startup.UpdateAppointConfig();
            MessageBox.Show("更新成功");
        }

        private void btnLegworkConfigUpdate_Click(object sender, EventArgs e)
        {
            Startup.UpdateLegworkGlobalConfig();
            MessageBox.Show("更新成功");
        }
    }
}

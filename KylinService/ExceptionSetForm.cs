using System;
using System.Windows.Forms;

namespace KylinService
{
    public partial class ExceptionSetForm : Form
    {
        public ExceptionSetForm()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            this.ckbDisplay.Checked = ExceptionLogConfig.Display;

            this.ckbWriteFile.Checked = ExceptionLogConfig.WriteFile;
        }

        protected void ckbDisplay_CheckedChanged(object sender, EventArgs e)
        {
            ExceptionLogConfig.Display = this.ckbDisplay.Checked;
        }

        protected void ckbWriteFile_CheckedChanged(object sender, EventArgs e)
        {
            ExceptionLogConfig.WriteFile = this.ckbWriteFile.Checked;
        }
    }
}

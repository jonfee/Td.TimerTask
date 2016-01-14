using KylinService.Core;
using KylinService.Data.Provider;
using System;
using System.Windows.Forms;

namespace KylinService.Services.ShakeDayTimes
{
    /// <summary>
    /// 摇一摇每日已摇次数清除计划
    /// </summary>
    public class ShakeTimesClearScheduler : IScheduler
    {
        public ShakeTimesClearScheduler(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate) { }
        protected override void Execute()
        {
            int count = ShakeProvider.ResetDayTimes();

            string message = string.Format("共对 {0} 位用户进行了摇一摇当日已摇次数清除", count);

            DelegateTool.WriteMessage(this.CurrentForm, this.WriteDelegate, message);
        }
    }
}

using KylinService.Core;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Td.Task.Framework;

namespace KylinService.Services.ShakeDayTimes
{
    public class ShakeTimesClearService : BaseService
    {
        public ShakeTimesClearService(Form form, DelegateTool.WriteMessageDelegate writeDelegate) : base(form, writeDelegate)
        {
            this.ServiceType = ScheduleType.ShakeDayTimesClear.ToString();
        }

        protected override void OnStart(params object[] parameters)
        {
            string beforeMessage = string.Format("{0} 计划正在执行中……", ServiceName);
            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, beforeMessage);

            var schedule = new ShakeTimesClearScheduler(this.CurrentForm, this.WriteDelegate);

            schedule.Start();

            string endMessage = string.Format("{0} 计划执行完成！", ServiceName);

            DelegateTool.WriteMessage(this.CurrentForm, WriteDelegate, endMessage);
        }
    }
}

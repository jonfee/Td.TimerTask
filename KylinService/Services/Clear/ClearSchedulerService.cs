﻿using KylinService.Core;
using KylinService.SysEnums;
using System.Windows.Forms;

namespace KylinService.Services.Clear
{
    /// <summary>
    /// 清理服务抽象类
    /// </summary>
    public abstract class ClearSchedulerService : SchedulerService
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public ClearScheduleType ServiceType;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="scheduleType"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public ClearSchedulerService(ClearScheduleType scheduleType)
        {
            ServiceType = scheduleType;

            ServiceName = SysData.GetClearServiceName((int)ServiceType);
        }
    }
}

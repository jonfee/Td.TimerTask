using KylinService.Core;
using KylinService.SysEnums;
using System.Windows.Forms;

namespace KylinService.Services.Queue
{
    /// <summary>
    /// 队列服务抽象类
    /// </summary>
    public abstract class QueueSchedulerService : SchedulerService
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public QueueScheduleType ServiceType;

        /// <summary>
        /// 初始化
        /// </summary>
        public QueueSchedulerService() : this(default(QueueScheduleType)) { }

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="scheduleType"></param>
        /// <param name="form"></param>
        /// <param name="writeDelegate"></param>
        public QueueSchedulerService(QueueScheduleType scheduleType) : base(true, 100)
        {
            ServiceType = scheduleType;

            ServiceName = SysData.GetQueueServiceName((int)ServiceType);
        }
    }
}

using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KylinService.Redis.Schedule
{
    /// <summary>
    /// 任务计划数据存储Redis配置采集器
    /// </summary>
    public sealed class ScheduleRedisCollection
    {
        public List<ScheduleRedisConfig> Items { get; set; }

        public ScheduleRedisConfig this[string scheduleName]
        {
            get
            {
                if (Contains(scheduleName))
                {
                    return Items.FirstOrDefault(p => p.ScheduleName.Equals(scheduleName, StringComparison.OrdinalIgnoreCase));
                }
                return null;
            }
            set
            {
                if (Contains(scheduleName))
                {
                    var item= Items.FirstOrDefault(p => p.ScheduleName.Equals(scheduleName, StringComparison.OrdinalIgnoreCase));
                    Items.Remove(item);
                    Items.Add(value);
                }
                else
                {
                    if (null == Items) Items = new List<ScheduleRedisConfig>();
                    Items.Add(value);
                }
            }
        }

        public ScheduleRedisConfig this[QueueScheduleType type]
        {
            get
            {
                if (Contains(type))
                {
                    return Items.FirstOrDefault(p => p.Type.Equals(type));
                }
                return null;
            }
            set
            {
                if (Contains(type))
                {
                    var item = Items.FirstOrDefault(p => p.Type.Equals(type));
                    Items.Remove(item);
                    Items.Add(value);
                }
                else
                {
                    if (null == Items) Items = new List<ScheduleRedisConfig>();
                    Items.Add(value);
                }
            }
        }

        public bool Contains(string scheduleName)
        {
            if (null != Items)
            {
                return Items.Count(p => p.ScheduleName.Equals(scheduleName, StringComparison.OrdinalIgnoreCase)) > 0;
            }

            return false;
        }

        public bool Contains(QueueScheduleType type)
        {
            if (null != Items)
            {
                return Items.Count(p => p.Type.Equals(type)) > 0;
            }

            return false;
        }
    }
}

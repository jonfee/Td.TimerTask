using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Td.Task.Framework
{
    /// <summary>
    /// 任务计划
    /// </summary>
    public class TaskSchedule
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        public static Hashtable Tasks = Hashtable.Synchronized(new Hashtable());
        
        /// <summary>
        /// 开始一个新的任务计划
        /// </summary>
        /// <param name="scheduleName">计划名称</param>
        /// <param name="task">任务执行者</param>
        /// <param name="strategy">运行时间策略</param>
        /// <param name="Params">启动参数</param>
        /// <returns></returns>
        public static ITask StartSchedule(string scheduleName, ITask task, TimerStrategy strategy, params object[] Params)
        {
            if (Tasks.ContainsKey(scheduleName))
                return null;

            task.Start(strategy, Params);
            Tasks.Add(scheduleName, task);

            return task;
        }

        /// <summary>
		/// 开始一个新的任务计划
		/// </summary>
		/// <param name="scheduleName">计划名称</param>
		/// <param name="task">任务执行者</param>
		/// <param name="strategy">运行时间策略</param>
		/// <param name="Params">启动参数</param>
		/// <returns></returns>
		public static ITask StartSchedule(string scheduleName, ITask task, string strategyName, params object[] Params)
        {
            if (Tasks.ContainsKey(scheduleName))
                return null;

            var strategy = TimerStrategyManager.StrategyConfig.Config.SingleOrDefault(e => e.RefName == strategyName);

            task.Start(strategy, Params);
            Tasks.Add(scheduleName, task);

            return task;
        }
        /// <summary>
        /// 开始一个新的任务计划
        /// </summary>
        /// <param name="scheduleName"></param>
        /// <param name="taskClassName"></param>
        /// <param name="strategyName"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static ITask StartSchedule(string scheduleName, string taskClassName, string strategyName, params object[] Params)
        {
            if (Tasks.ContainsKey(scheduleName))
                return null;

            var assemblyName = taskClassName.Split(new char[] { ',', ' ' })[1];
            var task = (ITask)Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName)).CreateInstance(taskClassName);
            var strategy = TimerStrategyManager.StrategyConfig.Config.SingleOrDefault(e => e.RefName == strategyName);

            task.Start(strategy, Params);
            Tasks.Add(scheduleName, task);

            return task;
        }

        /// <summary>
		/// 停止所有的计划任务
		/// </summary>
		public static void StopAllTask()
        {
            foreach (var item in Tasks)
            {
                try
                {
                    var task = Tasks[item] as ITask;
                    task.Stop();
                }
                catch { }
            }

            Tasks.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Td.Task.Framework
{
    public abstract class ITask
    {
        #region 私有成员

        /// <summary>
        /// 定时器
        /// </summary>
        private Timer _SysTimer = null;

        /// <summary>
        /// 定时器策略
        /// </summary>
        private TimerStrategy _Strategy = null;

        /// <summary>
        /// 任务启动参数
        /// </summary>
        private object[] _Params = null;

        #endregion

        /// <summary>
        /// 当前任务状态
        /// </summary>
        public TimerTaskStatus TaskStatus { get; set; }

        /// <summary>
        /// 任务计划策略
        /// </summary>
        public TimerStrategy ScheduleStrategy
        {
            get
            {
                return _Strategy;
            }
            set
            {
                TimeCalculator.CheckStrategy(value);

                this._Strategy = value;
            }
        }

        #region 服务操作

        /// <summary>
        /// 服务开始
        /// </summary>
        public void Start(TimerStrategy strategy, params object[] parameters)
        {
            try
            {
                TimeCalculator.CheckStrategy(strategy);

                this._Params = parameters;

                this.ScheduleStrategy = strategy;

                if (this.ScheduleStrategy.Delay == null)
                {
                    this.ScheduleStrategy.Delay = new TimeSpan(0);
                }

                this._SysTimer = new Timer(new TimerCallback(this.TimerIntervalRunning), AppDomain.CurrentDomain, this.ScheduleStrategy.Delay, this.ScheduleStrategy.Interval);
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        public void Stop()
        {
            try
            {
                OnStop();
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }

            if (this._SysTimer != null)
            {
                this._SysTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this._SysTimer.Dispose();
                this._SysTimer = null;
            }

            this.TaskStatus = TimerTaskStatus.Stopped;
        }

        /// <summary>
        /// 单次执行
        /// </summary>
        public void RunningOnce()
        {
            try
            {
                this.OnStart(this._Params);
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);
            }
        }

        /// <summary>
        /// 间隔执行
        /// </summary>
        public void TimerIntervalRunning(object sender)
        {
            //检测是否到了执行服务程序的时间
            bool timeIsUp = true;

            if (this.ScheduleStrategy.TimerMode != TimerMode.Interval)
            {
                timeIsUp = TimeCalculator.TimeIsUp(this.ScheduleStrategy);
            }

            try
            {
                //时间到
                if (timeIsUp)
                {
                    //服务运行中
                    TaskStatus = TimerTaskStatus.Running;

                    //设置计时器，在无穷时间后再启用（实际上已经停止计时器计时了）
                    this._SysTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    //开始处理
                    if (this.ScheduleStrategy.ReEntrant)
                    {
                        new Action(() =>
                        {
                            try
                            {
                                this.OnStart(this._Params);
                            }
                            catch (Exception ex)
                            {
                                this.OnThrowException(ex);//处理异常
                            }
                        }).BeginInvoke(null, null);
                    }
                    else
                    {
                        this.OnStart(this._Params);
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnThrowException(ex);//处理异常
            }
            finally
            {
                //计时器存在，则重置休眠时间
                if (null != this._SysTimer)
                {
                    if (this.ScheduleStrategy.TimerMode == TimerMode.Interval)
                    {
                        //重新启用计时器
                        this._SysTimer.Change(this.ScheduleStrategy.Interval, this.ScheduleStrategy.Interval);
                    }
                    else
                    {
                        var interval = TimeCalculator.GetNextTimeUp(this.ScheduleStrategy);

                        if (interval.Ticks <= 0)
                        {
                            interval = TimeCalculator.GetNextTimeUp(this.ScheduleStrategy);
                        }

                        //重置
                        this._SysTimer.Change(interval, interval);
                    }
                }

                TaskStatus = TimerTaskStatus.Completed;
            }
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 开始服务
        /// </summary>
        protected abstract void OnStart(params object[] parameters);

        /// <summary>
        /// 停止服务
        /// </summary>
        protected abstract void OnStop();

        /// <summary>
        /// 产生异常时处理
        /// </summary>
        /// <param name="ex"></param>
        protected abstract void OnThrowException(Exception ex);

        #endregion
    }
}

// -----------------------------------------------------------------------
// <copyright file="Scheduler.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System.Threading.Tasks;

    /// <summary>
    /// Scheduler contains various method including:
    /// 1. Schedule a task by a fixed time interval;
    /// 2. Schedule a task by a fixed frame interval;
    /// 3. Register and unregister callbacks to Unity player loop update events;
    /// 4. Send or post callbacks to Unity main thread;
    /// 5. Start and stop a Unity coroutine.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Gets a value indicating whether the SchedulerImpl instance exists.
        /// </summary>
        public static bool HasInstance => SchedulerImpl.HasInstance;

        /// <summary>
        /// Gets or sets the global time scale for <see cref="Scheduler"/>.
        /// </summary>
        public static float GlobalTimeScale
        {
            get => SchedulerImpl.Instance.globalTimeScale;
            set => SchedulerImpl.Instance.globalTimeScale = value;
        }

        /// <summary>
        /// Gets the current update phase.
        /// </summary>
        public static int UpdatePhase
        {
            get => SchedulerImpl.Instance.updatePhase;
        }

        /// <summary>
        /// Gets or sets max concurrency for threaded tasks.
        /// </summary>
        public static int ThreadedTasksMaxConcurrency
        {
            get
            {
                if (SchedulerImpl.Instance.taskFactory != null)
                {
                    TaskScheduler taskScheduler = SchedulerImpl.Instance.taskFactory.Scheduler;
                    if (taskScheduler is SimpleTaskScheduler simpleTaskScheduler)
                    {
                        return simpleTaskScheduler.threadedTasksMaxConcurrency;
                    }
                }

                return SchedulerImpl.Instance.threadedTasksMaxConcurrency;
            }

            set
            {
                if (SchedulerImpl.Instance.taskFactory != null)
                {
                    TaskScheduler taskScheduler = SchedulerImpl.Instance.taskFactory.Scheduler;
                    if (taskScheduler is SimpleTaskScheduler simpleTaskScheduler)
                    {
                        simpleTaskScheduler.threadedTasksMaxConcurrency = value;
                    }
                }

                SchedulerImpl.Instance.threadedTasksMaxConcurrency = value;
            }
        }

        /// <summary>
        /// Get current running info for debug purpose.
        /// </summary>
        /// <returns>Running info in Json format.</returns>
        public static string GetRunningInfo()
        {
            if (!SchedulerImpl.HasInstance)
            {
                return string.Empty;
            }

            SchedulerImpl ins = SchedulerImpl.Instance;

            return $"{{ " +

                $"\"GlobalTimeScale\" :{ins.globalTimeScale}, " +
                $"\"UpdatePhase\" : {ins.updatePhase}," +

                $"\"Timing Tasks\" :{ins.managedTasks.Count}," +
                $"\"Timing Tasks Unscaled\" :{ins.managedTasksUnscaled.Count}," +

                $"\"Frame Tasks\" :{ins.managedFrameTasks.Count}," +

                $"\"EarlyUpdate\" :{ins.earlyUpdate.ListenerCount}," +
                $"\"PreUpdate\" :{ins.preUpdate.ListenerCount}," +
                $"\"FixedUpdate\" :{ins.fixedUpdate.ListenerCount}," +
                $"\"Update\" :{ins.update.ListenerCount}," +
                $"\"PreLateUpdate\" :{ins.preLateUpdate.ListenerCount}," +
                $"\"LateUpdate\" :{ins.lateUpdate.ListenerCount}," +
                $"\"PostLateUpdate\" :{ins.postLateUpdate.ListenerCount}" +

                $" }}";
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="Scheduler.TimingTasks.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;

    /// <summary>
    /// Scheduler methods related to timing task.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Schedule a task once.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleOnce(Action action, float delay)
        {
            return CreateTask(action, 1, 0, delay, false);
        }

        /// <summary>
        /// Schedule a task.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWithDelay(Action action, float interval, float delay)
        {
            return CreateTask(action, -1, interval, delay, false);
        }

        /// <summary>
        /// Schedule a task.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask Schedule(Action action, float interval)
        {
            return CreateTask(action, -1, interval, 0, false);
        }

        /// <summary>
        /// Schedule a task.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWithDelay(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, times, interval, delay, false);
        }

        /// <summary>
        /// Schedule a task.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask Schedule(Action action, int times, float interval)
        {
            return CreateTask(action, times, interval, 0, false);
        }

        /// <summary>
        /// Schedule a task once ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleOnceUnscaled(Action action, float delay)
        {
            return CreateTask(action, 1, 0, delay, true);
        }

        /// <summary>
        /// Schedule a task once ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWithDelayUnscaled(Action action, float interval, float delay)
        {
            return CreateTask(action, -1, interval, delay, true);
        }

        /// <summary>
        /// Schedule a task once ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleUnscaled(Action action, float interval)
        {
            return CreateTask(action, -1, interval, 0, true);
        }

        /// <summary>
        /// Schedule a task once ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWithDelayUnscaled(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, times, interval, delay, true);
        }

        /// <summary>
        /// Schedule a task once ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleUnscaled(Action action, int times, float interval)
        {
            return CreateTask(action, times, interval, 0, true);
        }

        /// <summary>
        /// Schedule a task while a condition is true.
        /// </summary>
        /// <param name="func">The condition to check.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWhile(Func<bool> func, float interval)
        {
            ScheduledTimingTask tsk = default;
            void action()
            {
                if (!func())
                {
                    tsk.Unschedule();
                }
            }

            tsk = CreateTask(
                action,
                -1,
                interval,
                0,
                false);
            return tsk;
        }

        /// <summary>
        /// Schedule a task while a condition is true.
        /// </summary>
        /// <param name="func">The condition to check.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWithDelayWhile(Func<bool> func, float interval, float delay)
        {
            ScheduledTimingTask tsk = default;
            void action()
            {
                if (!func())
                {
                    tsk.Unschedule();
                }
            }

            tsk = CreateTask(
                action,
                -1,
                interval,
                delay,
                false);
            return tsk;
        }

        /// <summary>
        /// Schedule a task while a condition is true ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="func">The condition to check.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleUnscaledWhile(Func<bool> func, float interval)
        {
            ScheduledTimingTask tsk = default;
            void action()
            {
                if (!func())
                {
                    tsk.Unschedule();
                }
            }

            tsk = CreateTask(
                action,
                -1,
                interval,
                0,
                true);
            return tsk;
        }

        /// <summary>
        /// Schedule a task while a condition is true ignoring <see cref="UnityEngine.Time.timeScale"/>>.
        /// </summary>
        /// <param name="func">The condition to check.</param>
        /// <param name="interval">Interval between executions.</param>
        /// <param name="delay">Time before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledTimingTask ScheduleWithDelayUnscaledWhile(Func<bool> func, float interval, float delay)
        {
            ScheduledTimingTask tsk = default;
            void action()
            {
                if (!func())
                {
                    tsk.Unschedule();
                }
            }

            tsk = CreateTask(
                action,
                -1,
                interval,
                delay,
                true);
            return tsk;
        }

        /// <summary>
        /// Unschedule a task.
        /// </summary>
        /// <param name="task">Task to unschedule.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool Unschedule(ScheduledTimingTask task)
        {
            if (task == null)
            {
                return false;
            }

            if (task.handle == null || task.removed)
            {
                return false;
            }

            task.removed = true;
            return true;
        }

        private static ScheduledTimingTask CreateTask(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            var task = new ScheduledTimingTask(5)
            {
                action = action,
                times = times,
                interval = interval,
                timer = -delay,
            };

            if (useUnscaledTime)
            {
                task.handle = SchedulerImpl.Instance.managedTasksUnscaled.AddLast(task);
            }
            else
            {
                task.handle = SchedulerImpl.Instance.managedTasks.AddLast(task);
            }

            return task;
        }
    }
}

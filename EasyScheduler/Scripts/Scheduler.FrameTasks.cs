// -----------------------------------------------------------------------
// <copyright file="Scheduler.FrameTasks.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;

    /// <summary>
    /// Scheduler methods related to frame task.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Schedule a task after frames.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="frames">Frames before first execution.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleAfterFrames(Action action, ushort frames)
        {
            return CreateFrameTask(action, ScheduleMode.Dynamic, 1, frames, 0);
        }

        /// <summary>
        /// Schedule a task next frames.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleNextFrame(Action action)
        {
            return CreateFrameTask(action, ScheduleMode.Dynamic, 1, 1, 0);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="mode">Mode to schedule <see cref="ScheduleMode"/>>.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrame(Action action, ScheduleMode mode, ushort frameInterval)
        {
            return CreateFrameTask(action, mode, -1, frameInterval, 0);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="mode">Mode to schedule <see cref="ScheduleMode"/>>.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrame(Action action, ScheduleMode mode, int times, ushort frameInterval)
        {
            return CreateFrameTask(action, mode, times, frameInterval, 0);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="mode">Mode to schedule <see cref="ScheduleMode"/>>.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <param name="initialPhase">Frame starting count in the first period.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrameWithInitialPhase(Action action, ScheduleMode mode, ushort frameInterval, int initialPhase)
        {
            return CreateFrameTask(action, mode, -1, frameInterval, initialPhase);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="mode">Mode to schedule <see cref="ScheduleMode"/>>.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <param name="initialPhase">Frame starting count in the first period.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrameWithInitialPhase(Action action, ScheduleMode mode, int times, ushort frameInterval, int initialPhase)
        {
            return CreateFrameTask(action, mode, times, frameInterval, initialPhase);
        }

        /// <summary>
        /// Unschedule a task.
        /// </summary>
        /// <param name="task">Task to unschedule.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool Unschedule(ScheduledFrameTask task)
        {
            switch (task)
            {
                case ScheduledFrameTaskDynamic taskDynamic:
                    return Unschedule(taskDynamic);
                case ScheduledFrameTaskStatic taskStatic:
                    return Unschedule(taskStatic);
            }

            return false;
        }

        /// <summary>
        /// Unschedule a task.
        /// </summary>
        /// <param name="task">Task to unschedule.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool Unschedule(ScheduledFrameTaskDynamic task)
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

        /// <summary>
        /// Unschedule a task.
        /// </summary>
        /// <param name="task">Task to unschedule.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool Unschedule(ScheduledFrameTaskStatic task)
        {
            if (task == null)
            {
                return false;
            }

            if (task.removed)
            {
                return false;
            }

            task.removed = true;
            return true;
        }

        private static ScheduledFrameTask CreateFrameTask(Action action, ScheduleMode mode, int times, ushort frameInterval, int initialPhase)
        {
            switch (mode)
            {
                case ScheduleMode.Dynamic:
                    return CreateFrameTaskDynamic(action, times, frameInterval, initialPhase);
                case ScheduleMode.Static:
                    return CreateFrameTaskStatic(action, times, frameInterval, initialPhase);
            }

            throw new NotSupportedException();
        }

        private static ScheduledFrameTaskDynamic CreateFrameTaskDynamic(Action action, int times, ushort frameInterval, int initialPhase)
        {
            var task = new ScheduledFrameTaskDynamic()
            {
                action = action,
                times = times,
                frameInterval = frameInterval,
                counter = initialPhase,
            };

            task.handle = SchedulerImpl.Instance.managedDynamicFrameTasks.AddLast(task);

            return task;
        }

        private static ScheduledFrameTaskStatic CreateFrameTaskStatic(Action action, int times, ushort frameInterval, int initialPhase)
        {
            var task = new ScheduledFrameTaskStatic()
            {
                action = action,
                times = times,
                frameInterval = frameInterval,
                counter = initialPhase,
            };

            SchedulerImpl.Instance.managedStaticFrameTasks.Add(task);

            return task;
        }
    }
}

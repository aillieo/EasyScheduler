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
            return CreateFrameTask(action, 1, frames, 0);
        }

        /// <summary>
        /// Schedule a task next frames.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleNextFrame(Action action)
        {
            return CreateFrameTask(action, 1, 1, 0);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrame(Action action, ushort frameInterval)
        {
            return CreateFrameTask(action, -1, frameInterval, 0);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrame(Action action, int times, ushort frameInterval)
        {
            return CreateFrameTask(action, times, frameInterval, 0);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <param name="initialPhase">Frame starting count in the first period.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrameWithInitialPhase(Action action, ushort frameInterval, int initialPhase)
        {
            return CreateFrameTask(action, -1, frameInterval, initialPhase);
        }

        /// <summary>
        /// Schedule a task by frame.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="times">Execution repeat times.</param>
        /// <param name="frameInterval">Frame count between executions.</param>
        /// <param name="initialPhase">Frame starting count in the first period.</param>
        /// <returns>Scheduled task.</returns>
        public static ScheduledFrameTask ScheduleByFrameWithInitialPhase(Action action, int times, ushort frameInterval, int initialPhase)
        {
            return CreateFrameTask(action, times, frameInterval, initialPhase);
        }

        /// <summary>
        /// Unschedule a task.
        /// </summary>
        /// <param name="task">Task to unschedule.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool Unschedule(ScheduledFrameTask task)
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

        private static ScheduledFrameTask CreateFrameTask(Action action, int times, ushort frameInterval, int initialPhase)
        {
            var task = new ScheduledFrameTask(5)
            {
                action = action,
                times = times,
                frameInterval = frameInterval,
                counter = initialPhase,
            };

            task.handle = SchedulerImpl.Instance.managedFrameTasks.AddLast(task);

            return task;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils
{
    public static partial class Scheduler
    {
        public static ScheduledFrameTask ScheduleAfterFrames(Action action, int frames)
        {
            return CreateFrameTask(action, ScheduleMode.Dynamic, 1, frames, 0);
        }

        public static ScheduledFrameTask ScheduleNextFrame(Action action)
        {
            return CreateFrameTask(action, ScheduleMode.Dynamic, 1, 1, 0);
        }

        public static ScheduledFrameTask ScheduleByFrame(Action action, ScheduleMode mode, int frameInterval)
        {
            return CreateFrameTask(action, mode, -1, frameInterval, 0);
        }

        public static ScheduledFrameTask ScheduleByFrame(Action action, ScheduleMode mode, int times, int frameInterval)
        {
            return CreateFrameTask(action, mode, times, frameInterval, 0);
        }

        public static ScheduledFrameTask ScheduleByFrameWithInitialPhase(Action action, ScheduleMode mode, int frameInterval, int initialPhase)
        {
            return CreateFrameTask(action, mode, -1, frameInterval, initialPhase);
        }

        public static ScheduledFrameTask ScheduleByFrameWithInitialPhase(Action action, ScheduleMode mode, int times, int frameInterval, int initialPhase)
        {
            return CreateFrameTask(action, mode, times, frameInterval, initialPhase);
        }

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
            task.handle.List.Remove(task.handle);
            task.handle = null;
            task.removed = true;
            return true;
        }

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

        private static ScheduledFrameTask CreateFrameTask(Action action, ScheduleMode mode, int times, int frameInterval, int initialPhase)
        {
            switch (mode)
            {
                case ScheduleMode.Dynamic:
                    return CreateFrameTaskDynamic(action, times, frameInterval, initialPhase);
                case ScheduleMode.Static:
                    return CreateFrameTaskStatic(action, times, frameInterval, initialPhase);
                case ScheduleMode.LongTerm:
                    throw new NotSupportedException();
            }

            return default;
        }

        private static ScheduledFrameTaskDynamic CreateFrameTaskDynamic(Action action, int times, int frameInterval, int initialPhase)
        {
            ScheduledFrameTaskDynamic task = new ScheduledFrameTaskDynamic()
            {
                action = action,
                times = times,
                frameInterval = frameInterval,
                counter = initialPhase,
            };

            task.handle = SchedulerImpl.Instance.managedDynamicFrameTasks.AddLast(task);

            return task;
        }

        private static ScheduledFrameTaskStatic CreateFrameTaskStatic(Action action, int times, int frameInterval, int initialPhase)
        {
            ScheduledFrameTaskStatic task = new ScheduledFrameTaskStatic()
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

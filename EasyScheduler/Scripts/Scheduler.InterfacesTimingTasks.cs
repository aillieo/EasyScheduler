using System;

namespace AillieoUtils
{
    public partial class Scheduler
    {
        public static ScheduledTimingTask ScheduleOnce(Action action, float delay)
        {
            return CreateTask(action, defaultScheduleMode, 1, 0, delay, false);
        }

        public static ScheduledTimingTask ScheduleOnce(Action action, ScheduleMode mode, float delay)
        {
            return CreateTask(action, mode, 1, 0, delay, false);
        }

        public static ScheduledTimingTask ScheduleWithDelay(Action action, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, delay, false);
        }

        public static ScheduledTimingTask ScheduleWithDelay(Action action, ScheduleMode mode, float interval, float delay)
        {
            return CreateTask(action, mode, -1, interval, delay, false);
        }

        public static ScheduledTimingTask Schedule(Action action, float interval)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, 0, false);
        }

        public static ScheduledTimingTask Schedule(Action action, ScheduleMode mode, float interval)
        {
            return CreateTask(action, mode, -1, interval, 0, false);
        }

        public static ScheduledTimingTask ScheduleWithDelay(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, delay, false);
        }

        public static ScheduledTimingTask ScheduleWithDelay(Action action, ScheduleMode mode, int times, float interval, float delay)
        {
            return CreateTask(action, mode, times, interval, delay, false);
        }

        public static ScheduledTimingTask Schedule(Action action, int times, float interval)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, 0, false);
        }

        public static ScheduledTimingTask Schedule(Action action, ScheduleMode mode, int times, float interval)
        {
            return CreateTask(action, mode, times, interval, 0, false);
        }

        public static ScheduledTimingTask ScheduleOnceUnscaled(Action action, float delay)
        {
            return CreateTask(action, defaultScheduleMode, 1, 0, delay, true);
        }

        public static ScheduledTimingTask ScheduleOnceUnscaled(Action action, ScheduleMode mode, float delay)
        {
            return CreateTask(action, mode, 1, 0, delay, true);
        }

        public static ScheduledTimingTask ScheduleWithDelayUnscaled(Action action, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, delay, true);
        }

        public static ScheduledTimingTask ScheduleWithDelayUnscaled(Action action, ScheduleMode mode, float interval, float delay)
        {
            return CreateTask(action, mode, -1, interval, delay, true);
        }

        public static ScheduledTimingTask ScheduleUnscaled(Action action, float interval)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, 0, true);
        }

        public static ScheduledTimingTask ScheduleUnscaled(Action action, ScheduleMode mode, float interval)
        {
            return CreateTask(action, mode, -1, interval, 0, true);
        }

        public static ScheduledTimingTask ScheduleWithDelayUnscaled(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, delay, true);
        }

        public static ScheduledTimingTask ScheduleWithDelayUnscaled(Action action, ScheduleMode mode, int times, float interval, float delay)
        {
            return CreateTask(action, mode, times, interval, delay, true);
        }

        public static ScheduledTimingTask ScheduleUnscaled(Action action, int times, float interval)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, 0, true);
        }

        public static ScheduledTimingTask ScheduleUnscaled(Action action, ScheduleMode mode, int times, float interval)
        {
            return CreateTask(action, mode, times, interval, 0, true);
        }

        public static bool Unschedule(ScheduledTimingTask task)
        {
            switch (task)
            {
                case ScheduledTimingTaskDynamic taskDynamic:
                    return Unschedule(taskDynamic);
                case ScheduledTimingTaskLongTerm taskLongTerm:
                    return Unschedule(taskLongTerm);
                case ScheduledTimingTaskStatic taskStatic:
                    return Unschedule(taskStatic);
            }
            return false;
        }

        public static bool Unschedule(ScheduledTimingTaskDynamic task)
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

        public static bool Unschedule(ScheduledTimingTaskStatic task)
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

        public static bool Unschedule(ScheduledTimingTaskLongTerm task)
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

        private static ScheduledTimingTask CreateTask(Action action, ScheduleMode mode, int times, float interval, float delay, bool useUnscaledTime)
        {
            switch (mode)
            {
                case ScheduleMode.Dynamic:
                    return CreateTaskDynamic(action, times, interval, delay, useUnscaledTime);
                case ScheduleMode.Static:
                    return CreateTaskStatic(action, times, interval, delay, useUnscaledTime);
                case ScheduleMode.LongTerm:
                    return CreateTaskLongTerm(action, times, interval, delay, useUnscaledTime);
            }

            return default;
        }

        private static ScheduledTimingTaskDynamic CreateTaskDynamic(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTimingTaskDynamic task = new ScheduledTimingTaskDynamic()
            {
                action = action,
                times = times,
                interval = interval,
                timer = -delay,
            };

            if (useUnscaledTime)
            {
                task.handle = Instance.managedDynamicTasksUnscaled.AddLast(task);
            }
            else
            {
                task.handle = Instance.managedDynamicTasks.AddLast(task);
            }

            return task;
        }

        private static ScheduledTimingTaskStatic CreateTaskStatic(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTimingTaskStatic task = new ScheduledTimingTaskStatic()
            {
                action = action,
                times = times,
                interval = interval,
                timer = -delay,
            };

            if (useUnscaledTime)
            {
                Instance.managedStaticTasksUnscaled.Add(task);
            }
            else
            {
                Instance.managedStaticTasks.Add(task);
            }

            return task;
        }

        private static ScheduledTimingTaskLongTerm CreateTaskLongTerm(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTimingTaskLongTerm task = new ScheduledTimingTaskLongTerm()
            {
                action = action,
                times = times,
                interval = interval,
                timer = -delay,
            };

            if (useUnscaledTime)
            {
                EnqueueLongTermTask(task, Instance.managedLongTermTasksUnscaled, ref Instance.topTimerUnscaled);
            }
            else
            {
                EnqueueLongTermTask(task, Instance.managedLongTermTasks, ref Instance.topTimer);
            }

            return task;
        }
    }
}

using System;

namespace AillieoUtils
{
    public partial class Scheduler
    {
        public static ScheduledTask ScheduleOnce(Action action, float delay)
        {
            return CreateTask(action, defaultScheduleMode, 1, 0, delay, false);
        }

        public static ScheduledTask ScheduleOnce(Action action, ScheduleMode mode, float delay)
        {
            return CreateTask(action, mode, 1, 0, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, ScheduleMode mode, float interval, float delay)
        {
            return CreateTask(action, mode, -1, interval, delay, false);
        }

        public static ScheduledTask Schedule(Action action, float interval)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, 0, false);
        }

        public static ScheduledTask Schedule(Action action, ScheduleMode mode, float interval)
        {
            return CreateTask(action, mode, -1, interval, 0, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, ScheduleMode mode, int times, float interval, float delay)
        {
            return CreateTask(action, mode, times, interval, delay, false);
        }

        public static ScheduledTask Schedule(Action action, int times, float interval)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, 0, false);
        }

        public static ScheduledTask Schedule(Action action, ScheduleMode mode, int times, float interval)
        {
            return CreateTask(action, mode, times, interval, 0, false);
        }

        public static ScheduledTask ScheduleOnceUnscaled(Action action, float delay)
        {
            return CreateTask(action, defaultScheduleMode, 1, 0, delay, true);
        }

        public static ScheduledTask ScheduleOnceUnscaled(Action action, ScheduleMode mode, float delay)
        {
            return CreateTask(action, mode, 1, 0, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, ScheduleMode mode, float interval, float delay)
        {
            return CreateTask(action, mode, -1, interval, delay, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, float interval)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, 0, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, ScheduleMode mode, float interval)
        {
            return CreateTask(action, mode, -1, interval, 0, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, ScheduleMode mode, int times, float interval, float delay)
        {
            return CreateTask(action, mode, times, interval, delay, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, int times, float interval)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, 0, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, ScheduleMode mode, int times, float interval)
        {
            return CreateTask(action, mode, times, interval, 0, true);
        }

        public static bool Unschedule(ScheduledTask task)
        {
            switch (task)
            {
                case ScheduledTaskDynamic taskDynamic:
                    return Unschedule(taskDynamic);
                case ScheduledTaskLongTerm taskLongTerm:
                    return Unschedule(taskLongTerm);
                case ScheduledTaskStatic taskStatic:
                    return Unschedule(taskStatic);
            }
            return false;
        }

        public static bool Unschedule(ScheduledTaskDynamic task)
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

        public static bool Unschedule(ScheduledTaskStatic task)
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

        public static bool Unschedule(ScheduledTaskLongTerm task)
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

        private static ScheduledTask CreateTask(Action action, ScheduleMode mode, int times, float interval, float delay, bool useUnscaledTime)
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

        private static ScheduledTaskDynamic CreateTaskDynamic(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTaskDynamic task = new ScheduledTaskDynamic()
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

        private static ScheduledTaskStatic CreateTaskStatic(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTaskStatic task = new ScheduledTaskStatic()
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

        private static ScheduledTaskLongTerm CreateTaskLongTerm(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTaskLongTerm task = new ScheduledTaskLongTerm()
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

using System;

namespace AillieoUtils
{
    public static partial class Scheduler
    {
        public static Handle SchedulePreUpdate(Action action)
        {
            return SchedulerImpl.Instance.preUpdate.AddListener(action);
        }

        public static bool UnschedulePreUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.preUpdate.Remove(handle);
        }

        public static int UnschedulePreUpdate(Action action)
        {
            return SchedulerImpl.Instance.preUpdate.RemoveListener(action);
        }

        public static Handle ScheduleUpdate(Action action)
        {
            return SchedulerImpl.Instance.update.AddListener(action);
        }

        public static bool UnscheduleUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.update.Remove(handle);
        }

        public static int UnscheduleUpdate(Action action)
        {
            return SchedulerImpl.Instance.update.RemoveListener(action);
        }

        public static Handle ScheduleLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.lateUpdate.AddListener(action);
        }

        public static bool UnscheduleLateUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.lateUpdate.Remove(handle);
        }

        public static int UnscheduleLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.lateUpdate.RemoveListener(action);
        }

        public static Handle ScheduleFixedUpdate(Action action)
        {
            return SchedulerImpl.Instance.fixedUpdate.AddListener(action);
        }

        public static bool UnscheduleFixedUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.fixedUpdate.Remove(handle);
        }

        public static int UnscheduleFixedUpdate(Action action)
        {
            return SchedulerImpl.Instance.fixedUpdate.RemoveListener(action);
        }
    }
}

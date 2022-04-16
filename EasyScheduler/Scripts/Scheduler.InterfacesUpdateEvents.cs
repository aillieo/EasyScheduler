using System;

namespace AillieoUtils
{
    public partial class Scheduler
    {
        public static Handle ScheduleEarlyUpdate(Action action)
        {
            return Instance.earlyUpdate.AddListener(action);
        }

        public static bool UnscheduleEarlyUpdate(Handle handle)
        {
            return Instance.earlyUpdate.Remove(handle);
        }

        public static int UnscheduleEarlyUpdate(Action action)
        {
            return Instance.earlyUpdate.RemoveListener(action);
        }

        public static Handle ScheduleUpdate(Action action)
        {
            return Instance.update.AddListener(action);
        }

        public static bool UnscheduleUpdate(Handle handle)
        {
            return Instance.update.Remove(handle);
        }

        public static int UnscheduleUpdate(Action action)
        {
            return Instance.update.RemoveListener(action);
        }

        public static Handle ScheduleLateUpdate(Action action)
        {
            return Instance.lateUpdate.AddListener(action);
        }

        public static bool UnscheduleLateUpdate(Handle handle)
        {
            return Instance.lateUpdate.Remove(handle);
        }

        public static int UnscheduleLateUpdate(Action action)
        {
            return Instance.lateUpdate.RemoveListener(action);
        }

        public static Handle ScheduleFixedUpdate(Action action)
        {
            return Instance.fixedUpdate.AddListener(action);
        }

        public static bool UnscheduleFixedUpdate(Handle handle)
        {
            return Instance.fixedUpdate.Remove(handle);
        }

        public static int UnscheduleFixedUpdate(Action action)
        {
            return Instance.fixedUpdate.RemoveListener(action);
        }
    }
}

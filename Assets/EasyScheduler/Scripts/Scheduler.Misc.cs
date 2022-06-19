using System;
using System.Threading;

namespace AillieoUtils
{
    public static partial class Scheduler
    {
        public static void Delay(Action action)
        {
            SchedulerImpl.Instance.delayTasks.Enqueue(action);
        }

        public static void Post(Action action)
        {
            SchedulerImpl.Instance.synchronizationContext.Post(_ => action(), null);
        }

        public static void Post(SendOrPostCallback callback, object state)
        {
            SchedulerImpl.Instance.synchronizationContext.Post(callback, state);
        }

        public static void Send(Action action)
        {
            SchedulerImpl.Instance.synchronizationContext.Send(_ => action(), null);
        }

        public static void Send(SendOrPostCallback callback, object state)
        {
            SchedulerImpl.Instance.synchronizationContext.Send(callback, state);
        }
    }
}

using System;
using System.Threading;

namespace AillieoUtils
{
    public partial class Scheduler
    {
        public static void Delay(Action action)
        {
            Instance.delayTasks.Enqueue(action);
        }

        public static void Post(Action action)
        {
            Instance.synchronizationContext.Post(_ => action(), null);
        }

        public static void Post(SendOrPostCallback callback, object arg)
        {
            Instance.synchronizationContext.Post(callback, arg);
        }

        public static void Send(Action action)
        {
            Instance.synchronizationContext.Send(_ => action(), null);
        }

        public static void Send(SendOrPostCallback callback, object arg)
        {
            Instance.synchronizationContext.Send(callback, arg);
        }
    }
}

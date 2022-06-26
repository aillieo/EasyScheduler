// -----------------------------------------------------------------------
// <copyright file="Scheduler.Misc.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Threading;

    /// <summary>
    /// Miscellaneous methods in Scheduler.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Invoke an callback on a later call stack, normally in 0 to 1 frame.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Delay(Action action)
        {
            SchedulerImpl.Instance.delayTasks.Enqueue(action);
        }

        /// <summary>
        /// Asynchronously send an callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Post(Action action)
        {
            SchedulerImpl.Instance.synchronizationContext.Post(_ => action(), null);
        }

        /// <summary>
        /// Asynchronously send an callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="callback">Callback to invoke.</param>
        /// <param name="state">Object passed to the callback.</param>
        public static void Post(SendOrPostCallback callback, object state)
        {
            SchedulerImpl.Instance.synchronizationContext.Post(callback, state);
        }

        /// <summary>
        /// Synchronously send an callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Send(Action action)
        {
            SchedulerImpl.Instance.synchronizationContext.Send(_ => action(), null);
        }

        /// <summary>
        /// Synchronously send an callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="callback">Callback to invoke.</param>
        /// <param name="state">Object passed to the callback.</param>
        public static void Send(SendOrPostCallback callback, object state)
        {
            SchedulerImpl.Instance.synchronizationContext.Send(callback, state);
        }
    }
}

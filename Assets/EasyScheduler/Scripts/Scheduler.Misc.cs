// -----------------------------------------------------------------------
// <copyright file="Scheduler.Misc.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Miscellaneous methods in Scheduler.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Invoke a callback on a later call stack, normally in 0 to 1 frame.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Delay(Action action)
        {
            Queue<Action> delayTasks = SchedulerImpl.Instance.delayQueues[1];
            delayTasks.Enqueue(action);
        }

        /// <summary>
        /// Asynchronously send a callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Post(Action action)
        {
            SchedulerImpl.Instance.synchronizationContext.Post(_ => action(), null);
        }

        /// <summary>
        /// Asynchronously send a callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="callback">Callback to invoke.</param>
        /// <param name="state">Object passed to the callback.</param>
        public static void Post(SendOrPostCallback callback, object state)
        {
            SchedulerImpl.Instance.synchronizationContext.Post(callback, state);
        }

        /// <summary>
        /// Synchronously send a callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Send(Action action)
        {
            SchedulerImpl.Instance.synchronizationContext.Send(_ => action(), null);
        }

        /// <summary>
        /// Synchronously send a callback to Unity main thread to invoke.
        /// </summary>
        /// <param name="callback">Callback to invoke.</param>
        /// <param name="state">Object passed to the callback.</param>
        public static void Send(SendOrPostCallback callback, object state)
        {
            SchedulerImpl.Instance.synchronizationContext.Send(callback, state);
        }

        /// <summary>
        /// Run a task in background thread which will be managed by <see cref="Scheduler"/>.
        /// </summary>
        /// <param name="func">Function to execute.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel this task.</param>
        /// <returns><see cref="Task{object}"/> created.</returns>
        public static Task<object> RunThreaded(Func<object> func, CancellationToken cancellationToken = default)
        {
            SchedulerImpl instance = SchedulerImpl.Instance;
            if (instance.taskFactory == null)
            {
                instance.taskFactory = new TaskFactory<object>(new SimpleTaskScheduler(instance.threadedTasksMaxConcurrency));
            }

            return instance.taskFactory.StartNew(func, cancellationToken);
        }
    }
}

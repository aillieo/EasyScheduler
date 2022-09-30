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
        /// Invoke an callback on a later call stack, normally in 0 to 1 frame.
        /// </summary>
        /// <param name="action">Callback to invoke.</param>
        public static void Delay(Action action)
        {
            Queue<Action> delayTasks = SchedulerImpl.Instance.delayQueues[1];
            delayTasks.Enqueue(action);
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

        /// <summary>
        /// Run a task in background thread which will be managed by <see cref="Scheduler"/>.
        /// </summary>
        /// <param name="func">Function to execute.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel this task.</param>
        /// <returns><see cref="Task{object}"/> created.</returns>
        public static Task<object> RunThreaded(Func<object> func, CancellationToken cancellationToken = default)
        {
            SchedulerImpl instance = SchedulerImpl.Instance;
            return InternalRunThreaded(instance, func, cancellationToken);
        }

        private static Task<object> InternalRunThreaded(SchedulerImpl instance, Func<object> func, CancellationToken cancellationToken)
        {
            if (instance.threadedTasksRunning < instance.threadedTasksMaxConcurrency)
            {
                Interlocked.Increment(ref instance.threadedTasksRunning);
                Task<object> task = Task.Factory.StartNew(func, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

                void ContinueFunc(Task tsk)
                {
                    Interlocked.Decrement(ref instance.threadedTasksRunning);
                    CheckAndExecuteThreadedTasks(instance);
                }

                task.ContinueWith(ContinueFunc, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
                task.ContinueWith(ContinueFunc, CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.Default);

                return task;
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();
                object FuncWrapper()
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var o = func();
                        tcs.SetResult(o);
                        return o;
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }

                    return default;
                }

                instance.threadedTasksQueue.Enqueue((FuncWrapper, cancellationToken));
                return tcs.Task;
            }
        }

        private static void CheckAndExecuteThreadedTasks(SchedulerImpl instance)
        {
            while (instance.threadedTasksRunning < instance.threadedTasksMaxConcurrency)
            {
                if (instance.threadedTasksQueue.TryDequeue(out (Func<object>, CancellationToken) result))
                {
                    (Func<object> f, CancellationToken c) = result;
                    InternalRunThreaded(instance, f, c);
                }
                else
                {
                    break;
                }
            }
        }
    }
}

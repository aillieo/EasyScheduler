// -----------------------------------------------------------------------
// <copyright file="SimpleTaskScheduler.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SimpleTaskScheduler : TaskScheduler
    {
        private readonly Queue<Task> taskQueue = new Queue<Task>();

        private int threadedTasksMaxConcurrencyValue;

        private int threadedTasksRunning;

        public int threadedTasksMaxConcurrency
        {
            get
            {
                return this.threadedTasksMaxConcurrencyValue;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (this.threadedTasksMaxConcurrencyValue == value)
                {
                    return;
                }

                if (value > this.threadedTasksMaxConcurrencyValue)
                {
                    this.threadedTasksMaxConcurrencyValue = value;
                    this.CheckAndExecute();
                }
                else
                {
                    this.threadedTasksMaxConcurrencyValue = value;
                }
            }
        }

        public override int MaximumConcurrencyLevel
        {
            get { return this.threadedTasksMaxConcurrencyValue; }
        }

        public SimpleTaskScheduler(int maxConcurrency)
        {
            if (maxConcurrency < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrency));
            }

            this.threadedTasksMaxConcurrencyValue = maxConcurrency;
        }

        internal void Dispose()
        {
            lock (this.taskQueue)
            {
                this.taskQueue.Clear();
            }
        }

        /// <inheritdoc />
        protected sealed override void QueueTask(Task task)
        {
            lock (this.taskQueue)
            {
                this.taskQueue.Enqueue(task);
            }

            this.CheckAndExecute();
        }

        /// <inheritdoc/>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(this.taskQueue, ref lockTaken);
                if (lockTaken)
                {
                    return this.taskQueue;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this.taskQueue);
                }
            }
        }

        private void CheckAndExecute()
        {
            lock (this.taskQueue)
            {
                if (this.taskQueue.Count == 0)
                {
                    return;
                }

                if (this.threadedTasksRunning >= this.threadedTasksMaxConcurrencyValue)
                {
                    return;
                }

                Interlocked.Increment(ref this.threadedTasksRunning);
                Task task = this.taskQueue.Dequeue();
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    this.TryExecuteTask(task);
                    Interlocked.Decrement(ref this.threadedTasksRunning);
                    this.CheckAndExecute();
                });
            }
        }
    }
}

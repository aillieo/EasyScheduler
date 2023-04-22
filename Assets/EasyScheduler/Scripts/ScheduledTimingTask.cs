// -----------------------------------------------------------------------
// <copyright file="ScheduledTimingTask.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Task scheduled by time.
    /// </summary>
    public class ScheduledTimingTask : IScheduledTask
    {
        internal LinkedListNode<ScheduledTimingTask> handle;

        internal int times;
        internal Action action;
        internal float interval;
        internal float timer;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        internal StackTrace creatingStackTrace;
#endif

        internal ScheduledTimingTask(int skipFrames = 0)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            this.creatingStackTrace = new StackTrace(skipFrames, true);
#endif
        }

        /// <summary>
        /// Gets or sets the local time scale for the task.
        /// </summary>
        public virtual float localTimeScale { get; set; } = 1;

        /// <summary>
        /// Gets a value indicating whether this task is finished.
        /// </summary>
        public bool isDone { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this task is removed.
        /// </summary>
        public bool removed { get; internal set; }

        /// <inheritdoc/>
        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }
}

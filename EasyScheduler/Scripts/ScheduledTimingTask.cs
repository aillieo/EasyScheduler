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
    public abstract class ScheduledTimingTask : IScheduledTask
    {
        internal int times;
        internal Action action;
        internal float interval;
        internal float timer;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        internal StackTrace creatingStackTrace;

        internal ScheduledTimingTask()
        {
            this.creatingStackTrace = new StackTrace(5, true);
        }
#endif

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

    /// <inheritdoc/>
    public class ScheduledTimingTaskDynamic : ScheduledTimingTask
    {
        internal LinkedListNode<ScheduledTimingTaskDynamic> handle;

        internal ScheduledTimingTaskDynamic()
        {
        }
    }

    /// <inheritdoc/>
    public class ScheduledTimingTaskStatic : ScheduledTimingTask
    {
        internal ScheduledTimingTaskStatic()
        {
        }
    }
}

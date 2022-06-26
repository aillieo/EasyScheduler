// -----------------------------------------------------------------------
// <copyright file="ScheduledFrameTask.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Task scheduled by frame.
    /// </summary>
    public abstract class ScheduledFrameTask : IScheduledTask
    {
        internal int times;
        internal Action action;
        internal ushort frameInterval;
        internal int counter;

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
    public class ScheduledFrameTaskDynamic : ScheduledFrameTask
    {
        internal LinkedListNode<ScheduledFrameTaskDynamic> handle;

        internal ScheduledFrameTaskDynamic()
        {
        }
    }

    /// <inheritdoc/>
    public class ScheduledFrameTaskStatic : ScheduledFrameTask
    {
        internal ScheduledFrameTaskStatic()
        {
        }
    }
}

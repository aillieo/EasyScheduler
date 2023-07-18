// -----------------------------------------------------------------------
// <copyright file="ScheduledFrameTask.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Task scheduled by frame.
    /// </summary>
    public class ScheduledFrameTask : IScheduledTask
    {
        internal LinkedListNode<ScheduledFrameTask> handle;

        internal int times;
        internal Action action;
        internal ushort frameInterval;
        internal int counter;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        internal StackTrace creatingStackTrace;
#endif

        internal ScheduledFrameTask(int skipFrames = 0)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            this.creatingStackTrace = new StackTrace(skipFrames, true);
#endif
        }

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

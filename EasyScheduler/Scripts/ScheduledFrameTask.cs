using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    public abstract class ScheduledFrameTask : IScheduledTask
    {
        internal int times;
        internal Action action;
        internal int frameInterval;
        internal int counter;
        public bool isDone { get; internal set; } = false;
        public bool removed { get; internal set; } = false;

        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }

    public class ScheduledFrameTaskDynamic : ScheduledFrameTask
    {
        internal LinkedListNode<ScheduledFrameTaskDynamic> handle;

        internal ScheduledFrameTaskDynamic()
        {
        }
    }

    public class ScheduledFrameTaskStatic : ScheduledFrameTask
    {
        internal ScheduledFrameTaskStatic()
        {
        }
    }
}
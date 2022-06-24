using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    public abstract class ScheduledTimingTask : IScheduledTask
    {
        internal int times;
        internal Action action;
        internal float interval;
        internal float timer;
        public virtual float localTimeScale { get; set; } = 1;
        public bool isDone { get; internal set; } = false;
        public bool removed { get; internal set; } = false;

        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }

    public class ScheduledTimingTaskDynamic : ScheduledTimingTask
    {
        internal LinkedListNode<ScheduledTimingTaskDynamic> handle;

        internal ScheduledTimingTaskDynamic()
        {
        }
    }

    public class ScheduledTimingTaskStatic : ScheduledTimingTask
    {
        internal ScheduledTimingTaskStatic()
        {
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    public abstract class ScheduledTask
    {
        internal int times;
        internal Action action;
        internal float interval;
        internal float timer;
        public bool isDone { get; internal set; } = false;
        public bool removed { get; internal set; } = false;
        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }

    public class ScheduledTaskDynamic : ScheduledTask
    {
        public float speedRate { get; set; } = 1;
        internal LinkedListNode<ScheduledTaskDynamic> handle;

        internal ScheduledTaskDynamic()
        {
        }
    }

    public class ScheduledTaskStatic : ScheduledTask
    {
        public float speedRate { get; set; } = 1;

        internal ScheduledTaskStatic()
        {
        }
    }

    public class ScheduledTaskLongTerm : ScheduledTask, IComparable<ScheduledTaskLongTerm>
    {
        internal ScheduledTaskLongTerm()
        {
        }

        public int CompareTo(ScheduledTaskLongTerm other)
        {
            return (interval - timer).CompareTo(other.interval - other.timer);
        }
    }
}

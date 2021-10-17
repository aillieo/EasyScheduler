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
        public float speedRate { get; set; } = 1;
        public bool isDone { get; internal set; } = false;
        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }

    public class ScheduledTaskDynamic : ScheduledTask
    {
        internal float timer;
        internal LinkedListNode<ScheduledTaskDynamic> handle;

        internal ScheduledTaskDynamic()
        {
        }
    }

    public class ScheduledTaskStatic : ScheduledTask
    {
        internal float timer;

        internal ScheduledTaskStatic()
        {
        }
    }

    public class ScheduledTaskLongTerm : ScheduledTask
    {
        internal double timer;

        internal ScheduledTaskLongTerm()
        {
        }
    }
}

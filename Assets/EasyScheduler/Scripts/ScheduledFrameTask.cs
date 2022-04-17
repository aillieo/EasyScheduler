using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    public abstract class ScheduledFrameTask
    {
        internal int times;
        internal Action action;
        internal int frameInterval;
        internal int counter;
        public bool isDone { get; internal set; } = false;
        public bool removed { get; internal set; } = false;
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

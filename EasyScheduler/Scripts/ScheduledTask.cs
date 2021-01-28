using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils
{
    public class ScheduledTask
    {
        internal float timer;
        internal int times;
        internal Action action;
        internal float interval;
        internal LinkedListNode<ScheduledTask> handle;
        public float speedRate { get; set; } = 1;
        public bool isDone { get; internal set; } = false;

        internal ScheduledTask()
        {

        }

        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils
{
    public class Task
    {
        internal float timer;
        internal int times;
        internal Action action;
        internal float interval;
        internal LinkedListNode<Task> handle;
        public float speedRate { get; set; } = 1;
        public bool isDone { get; internal set; } = false;

        internal Task()
        {

        }

        public bool Unschedule()
        {
            return Scheduler.Unschedule(this);
        }
    }
}


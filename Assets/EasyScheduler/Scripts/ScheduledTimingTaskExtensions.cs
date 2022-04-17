using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    namespace AillieoUtils
    {
        public static class ScheduledTimingTaskExtensions
        {
            public static bool Unschedule(this ScheduledTimingTask scheduledTimingTask)
            {
                return Scheduler.Unschedule(scheduledTimingTask);
            }
        }
    }
}

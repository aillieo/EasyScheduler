using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace AillieoUtils.Tests
{
    [Category("Performance")]
    public class Performance
    {
        [UnityTest]
        public IEnumerator ScheduleMassiveTasks()
        {
            int rest = 1000000;

            for (int i = 0; i < 100000; i ++)
            {
                int times = (i % 17) + (i % 3) + 5;
                ushort interval = (ushort)((i % 10) + 1);

                Profiler.BeginSample("Perf-SchedulerImpl.AddTasks");

                Scheduler.ScheduleByFrame(() => rest--, times, interval);

                Profiler.EndSample();
            }

            yield return new WaitWhile(() => SchedulerImpl.Instance.managedFrameTasks.Count > 50000);

            for (int i = 0; i < 50000; i++)
            {
                int times = (i % 301) + (i % 31) + 10;
                ushort interval = (ushort)((i % 10) + 1);

                Profiler.BeginSample("Perf-SchedulerImpl.AddTasks");

                Scheduler.ScheduleByFrame(() => rest--, times, interval);

                Profiler.EndSample();
            }

            yield return new WaitWhile(() => rest > -1000000);
        }
    }
}

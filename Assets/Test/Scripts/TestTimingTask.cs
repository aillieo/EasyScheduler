using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace AillieoUtils.Tests
{
    [Category("TestTimingTask")]
    public class TestTimingTask
    {
        [UnityTest]
        public IEnumerator TestScheduleOnce()
        {
            int rest = 1;

            int n = 0;
            Scheduler.ScheduleOnce(
                () =>
                {
                    n++;
                }, 0.2f);

            Scheduler.ScheduleOnce(
                () =>
                {
                    Assert.AreEqual(n, 1);
                    Scheduler.ScheduleOnce(
                        () =>
                        {
                            Assert.AreEqual(n, 1);
                            rest--;
                        }, 0.2f);
                }, 0.2f);

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestSchedule1()
        {
            int rest = 5;

            int a0 = 0;
            int a1 = 0;
            Scheduler.Schedule(
                () =>
                {
                    a0++;
                }, 3, 0.1f);
            Scheduler.Schedule(
                () =>
                {
                    a1++;
                    if (a1 <= 3)
                    {
                        Assert.AreEqual(a1, a0);
                        rest--;
                    }
                    else
                    {
                        Assert.AreNotEqual(a1, a0);
                        rest--;
                    }
                }, 5, 0.1f);

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestSchedule2()
        {
            int rest = 6;

            int a2 = 0;
            int a3 = 0;
            Scheduler.Schedule(
                () =>
                {
                    a2++;
                }, 10, 0.5f);
            Scheduler.Schedule(
                () =>
                {
                    a3++;
                }, 20, 0.25f);
            Scheduler.Schedule(
                () =>
                {
                    Assert.AreEqual(a2 * 2, a3);
                    rest--;
                }, 6, 1);

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestSchedule3()
        {
            int rest = 20;

            int a4 = 0;
            int a5 = 0;
            Scheduler.Schedule(
                () =>
                {
                    a4++;
                }, 10, 0.4f);
            var t = Scheduler.Schedule(
                () =>
                {
                    a5++;
                }, 20, 0.4f);
            Scheduler.ScheduleOnce(
                () =>
                {
                    Scheduler.Unschedule(t);
                }, 4);

            Scheduler.Schedule(
                () =>
                {
                    Assert.AreEqual(a4, a5);
                    rest--;
                }, 20, 0.2f);

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestUnschedule1()
        {
            int rest = 1;

            int n = 0;
            ScheduledTimingTask tsk = default;
            tsk = Scheduler.Schedule(
                () =>
                {
                    n++;
                    tsk.Unschedule();
                }, Scheduler.ScheduleMode.Dynamic, 0.001f);

            yield return new WaitForSeconds(1);

            rest--;

            Assert.AreEqual(1, n);

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestUnschedule2()
        {
            int rest = 1;

            int n = 0;
            ScheduledTimingTask tsk = default;
            tsk = Scheduler.Schedule(
                () =>
                {
                    n++;
                    tsk.Unschedule();
                }, Scheduler.ScheduleMode.Dynamic, 0.001f);

            yield return new WaitForSeconds(1);

            rest--;

            Assert.AreEqual(1, n);

            yield return new WaitWhile(() => rest > 0);
        }
    }
}

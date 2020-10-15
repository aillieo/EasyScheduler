using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using ATask = AillieoUtils.Task;
using STask = System.Threading.Tasks.Task;

namespace AillieoUtils.Tests
{
    public class TestScheduler : MonoBehaviour
    {

        private int updateTimes = 0;
        private int lateUpdateTimes = 0;
        private int fixedUpdateTimes = 0;

        private void Start()
        {
            //TestWithExceptions();

            TestPost();
            TestScheduleOnce();
            TestSchedule1();
            TestSchedule2();
            TestSchedule3();

            this.updateTimes = 0;
            this.lateUpdateTimes = 0;
            this.fixedUpdateTimes = 0;
            TestScheduleUpdate();
            TestScheduleLateUpdate();
            TestScheduleFixedUpdate();
        }

        private void TestPost()
        {
            var main = Thread.CurrentThread.ManagedThreadId;
            STask task = STask.Factory.StartNew(() => {
                var newThread = Thread.CurrentThread.ManagedThreadId;
                Scheduler.Post(() => {
                    AssertAreNotEqual(newThread, Thread.CurrentThread.ManagedThreadId);
                    AssertAreEqual(main, Thread.CurrentThread.ManagedThreadId);
                });
            });

            Scheduler.Post(() => {
                AssertAreEqual(main, Thread.CurrentThread.ManagedThreadId);
            });
        }

        private void TestScheduleOnce()
        {
            int n = 0;
            Scheduler.ScheduleOnce(() => {
                n++;
            }, 0.2f);

            Scheduler.ScheduleOnce(() => {
                AssertAreEqual(n, 1);
                Scheduler.ScheduleOnce(() => {
                    AssertAreEqual(n, 1);
                }, 0.2f);
            }, 0.2f);
        }

        private void TestSchedule1()
        {
            int a0 = 0;
            int a1 = 0;
            Scheduler.Schedule(() => {
                a0++;
            }, 3, 0.1f);
            Scheduler.Schedule(() => {
                a1++;
                if(a1 <= 3)
                {
                    AssertAreEqual(a1, a0);
                }
                else
                {
                    AssertAreNotEqual(a1, a0);
                }
            }, 10, 0.1f);
        }

        private void TestSchedule2()
        {
            int a2 = 0;
            int a3 = 0;
            Scheduler.Schedule(() => {
                a2++;
            }, 10, 0.5f);
            Scheduler.Schedule(() => {
                a3++;
            }, 20, 0.25f);
            Scheduler.Schedule(() => {
                AssertAreEqual(a2 * 2, a3);
            }, 6, 1);
        }

        private void TestSchedule3()
        {
            int a4 = 0;
            int a5 = 0;
            Scheduler.Schedule(() => {
                a4++;
            }, 10, 0.4f);
            var t = Scheduler.Schedule(() => {
                a5++;
            }, 20, 0.4f);
            Scheduler.ScheduleOnce(() => {
                Scheduler.Unschedule(t);
            }, 4);

            Scheduler.Schedule(() => {
                AssertAreEqual(a4, a5);
            }, 20, 0.2f);
        }

        private void TestScheduleUpdate()
        {
            Scheduler.ScheduleUpdate(() => {
                this.updateTimes++;
            });
        }

        private void TestScheduleLateUpdate()
        {
            Scheduler.ScheduleLateUpdate(() => {
                this.lateUpdateTimes++;
            });
        }

        private void TestScheduleFixedUpdate()
        {
            Scheduler.ScheduleFixedUpdate(() => {
                this.fixedUpdateTimes++;
            });
        }

        private void TestWithExceptions()
        {
            Scheduler.Schedule(() =>
            {
                throw new System.Exception() { };
            }, 0.1f);
            Scheduler.ScheduleUpdate(() =>
            {
                throw new System.Exception() { };
            });
        }

        private void Update()
        {
            updateTimes --;
            AssertAreEqual(updateTimes, 0);
        }

        private void LateUpdate()
        {
            lateUpdateTimes--;
            AssertAreEqual(lateUpdateTimes, 0);
        }

        private void FixedUpdate()
        {
            fixedUpdateTimes--;
            AssertAreEqual(fixedUpdateTimes, 0);
        }

        private int testCount = 0;
        private void AssertAreEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
            testCount++;
            Debug.Log($"[{Time.realtimeSinceStartup}] {testCount}");
        }
        private void AssertAreNotEqual<T>(T expected, T actual)
        {
            Assert.AreNotEqual(expected, actual);
            testCount++;
            Debug.Log($"[{Time.realtimeSinceStartup}] {testCount}");
        }
    }

}

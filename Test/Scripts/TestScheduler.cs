using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace AillieoUtils.Tests
{
    [Category("Scheduler")]
    public class TestScheduler
    {
        [UnityTest]
        public IEnumerator TestPost()
        {
            int rest = 2;

            int main = Thread.CurrentThread.ManagedThreadId;
            Task task = Task.Factory.StartNew(() =>
            {
                int newThread = Thread.CurrentThread.ManagedThreadId;
                Scheduler.Post(() =>
                {
                    Assert.AreNotEqual(newThread, Thread.CurrentThread.ManagedThreadId);
                    Assert.AreEqual(main, Thread.CurrentThread.ManagedThreadId);
                    Interlocked.Decrement(ref rest);
                });
            });

            Scheduler.Post(() =>
            {
                Assert.AreEqual(main, Thread.CurrentThread.ManagedThreadId);
                Interlocked.Decrement(ref rest);
            });

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestSend()
        {
            int rest = 3;

            int n = 0;
            int main = Thread.CurrentThread.ManagedThreadId;
            Task task = Task.Factory.StartNew(() => {
                int newThread = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(n, 0);
                Interlocked.Increment(ref n);
                Assert.AreEqual(n, 1);
                Scheduler.Send(() => {
                    Assert.AreNotEqual(newThread, Thread.CurrentThread.ManagedThreadId);
                    Assert.AreEqual(main, Thread.CurrentThread.ManagedThreadId);
                    Assert.AreEqual(n, 1);
                    Interlocked.Increment(ref n);
                    Assert.AreEqual(n, 2);
                    Interlocked.Decrement(ref rest);
                });
                Assert.AreEqual(n, 2);
                Interlocked.Increment(ref n);
                Assert.AreEqual(n, 3);
                Scheduler.Send(() => {
                    Assert.AreNotEqual(newThread, Thread.CurrentThread.ManagedThreadId);
                    Assert.AreEqual(main, Thread.CurrentThread.ManagedThreadId);
                    Assert.AreEqual(n, 3);
                    Interlocked.Increment(ref n);
                    Assert.AreEqual(n, 4);
                    Interlocked.Decrement(ref rest);
                });
                Assert.AreEqual(newThread, Thread.CurrentThread.ManagedThreadId);
                Interlocked.Decrement(ref rest);
            });

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestDelay()
        {
            int rest = 1;

            int n = 0;
            Scheduler.Delay(() => { n++; });
            Assert.AreEqual(n, 0);
            Scheduler.Delay(() => {
                Assert.AreEqual(n, 1);
                rest--;
            });

            yield return new WaitWhile(() => rest > 0);
        }

        [UnityTest]
        public IEnumerator TestScheduleOnce()
        {
            int rest = 1;

            int n = 0;
            Scheduler.ScheduleOnce(() => {
                n++;
                rest--;
            }, 0.2f);

            Scheduler.ScheduleOnce(() => {
                Assert.AreEqual(n, 1);
                Scheduler.ScheduleOnce(() => {
                    Assert.AreEqual(n, 1);
                    rest--;
                }, 0.2f);
            }, 0.2f);

            yield return new WaitWhile(() => rest > 0);
        }

        [Test]
        public void TestSchedule1()
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
                    Assert.AreEqual(a1, a0);
                }
                else
                {
                    Assert.AreNotEqual(a1, a0);
                }
            }, 10, 0.1f);
        }

        [Test]
        public void TestSchedule2()
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
                Assert.AreEqual(a2 * 2, a3);
            }, 6, 1);
        }

        [Test]
        public void TestSchedule3()
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
                Assert.AreEqual(a4, a5);
            }, 20, 0.2f);
        }

        [Test]
        public void TestWithExceptions()
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
    }
}

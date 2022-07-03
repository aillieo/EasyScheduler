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
    [Category("TestMisc")]
    public class TestMisc
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
    }
}

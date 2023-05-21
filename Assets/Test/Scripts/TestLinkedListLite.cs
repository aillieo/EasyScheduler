using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AillieoUtils.Tests
{
    internal class TestLinkedListLite
    {
        [Test]
        public void TestAdd1()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            Assert.AreEqual(list.Count, 0);
            LinkedListLite<int>.Node handle = list.Add(5);
            Assert.AreEqual(handle.value, 5);
            Assert.AreEqual(list.Head.value, 5);
            Assert.AreEqual(list.Count, 1);
            Assert.AreEqual(list.Next(handle), null);
            Assert.AreEqual(list.AllocatedNodeCount, 1);
        }

        [Test]
        public void TestAdd2()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            Assert.AreEqual(list.Count, 0);
            LinkedListLite<int>.Node handle0 = list.Add(8);
            LinkedListLite<int>.Node handle1 = list.Add(9);

            Assert.AreEqual(handle0.value, 8);
            Assert.AreEqual(handle1.value, 9);
            Assert.AreEqual(list.Head.value, 8);
            Assert.AreEqual(list.Next(handle0), handle1);
            Assert.AreEqual(list.Next(handle1), null);
            Assert.AreEqual(list.AllocatedNodeCount, 2);
        }

        [Test]
        public void TestRemoveEnd()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);

            Assert.AreEqual(list.Count, 3);

            list.Remove(handle2);

            Assert.AreEqual(list.Count, 2);

            Assert.AreEqual(list.Head, handle0);
            Assert.AreEqual(handle0.value, 2);
            Assert.AreEqual(handle1.value, 4);

            Assert.AreEqual(list.Next(handle0), handle1);
            Assert.AreEqual(list.Next(handle1), null);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveHead()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);

            Assert.AreEqual(list.Count, 3);

            list.Remove(handle0);

            Assert.AreEqual(list.Count, 2);

            Assert.AreEqual(list.Head, handle1);
            Assert.AreEqual(handle1.value, 4);
            Assert.AreEqual(handle2.value, 6);

            Assert.AreEqual(list.Next(handle1), handle2);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveMid()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);

            Assert.AreEqual(list.Count, 3);

            list.Remove(handle1);

            Assert.AreEqual(list.Count, 2);

            Assert.AreEqual(list.Head, handle0);
            Assert.AreEqual(handle0.value, 2);
            Assert.AreEqual(handle2.value, 6);

            Assert.AreEqual(list.Next(handle0), handle2);
            Assert.AreEqual(list.Next(handle2), null);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveAll1()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);

            list.Remove(handle0);
            Assert.AreEqual(list.Head, handle1);
            list.Remove(handle1);
            Assert.AreEqual(list.Head, handle2);
            list.Remove(handle2);
            Assert.AreEqual(list.Count, 0);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveAll2()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);

            list.Remove(handle1);
            Assert.AreEqual(list.Head, handle0);
            list.Remove(handle0);
            Assert.AreEqual(list.Head, handle2);
            list.Remove(handle2);
            Assert.AreEqual(list.Count, 0);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveThenAdd1()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);
            list.Remove(handle0);

            LinkedListLite<int>.Node handle3 = list.Add(8);
            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list.Head, handle1);
            Assert.AreEqual(list.Next(handle1), handle2);
            Assert.AreEqual(list.Next(handle2), handle3);
            Assert.AreEqual(list.Next(handle3), null);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveThenAdd2()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);
            list.Remove(handle1);

            LinkedListLite<int>.Node handle3 = list.Add(8);
            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list.Head, handle0);
            Assert.AreEqual(list.Next(handle0), handle2);
            Assert.AreEqual(list.Next(handle2), handle3);
            Assert.AreEqual(list.Next(handle3), null);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        [Test]
        public void TestRemoveThenAdd3()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            LinkedListLite<int>.Node handle0 = list.Add(2);
            LinkedListLite<int>.Node handle1 = list.Add(4);
            LinkedListLite<int>.Node handle2 = list.Add(6);
            list.Remove(handle2);

            LinkedListLite<int>.Node handle3 = list.Add(8);
            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list.Head, handle0);
            Assert.AreEqual(list.Next(handle0), handle1);
            Assert.AreEqual(list.Next(handle1), handle3);
            Assert.AreEqual(list.Next(handle3), null);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
        }

        private static int[] indexToRemove = new int[] { 0, 1, 2, 3 };

        [Test]
        public void TestAddRemoveA([ValueSource(nameof(indexToRemove))]int index)
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            List<LinkedListLite<int>.Node> nodes = new List<LinkedListLite<int>.Node>();

            nodes.Add(list.Add(1));
            nodes.Add(list.Add(2));
            nodes.Add(list.Add(3));
            nodes.Add(list.Add(4));
            Assert.AreEqual(list.AllocatedNodeCount, 4);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            nodes.Add(list.Add(5));
            nodes.Add(list.Add(6));
            Assert.AreEqual(list.AllocatedNodeCount, 5);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            int flip = list.Count - 1 - index;
            list.Remove(nodes[flip]);
            nodes.RemoveAt(flip);

            nodes.Add(list.Add(7));
            nodes.Add(list.Add(8));
            nodes.Add(list.Add(9));
            Assert.AreEqual(list.AllocatedNodeCount, 6);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            list.Remove(nodes[flip]);
            nodes.RemoveAt(flip);
            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            LinkedListLite<int>.Node current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }
        }

        [Test]
        public void TestAddRemoveB([ValueSource(nameof(indexToRemove))] int index)
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            List<LinkedListLite<int>.Node> nodes = new List<LinkedListLite<int>.Node>();

            nodes.Add(list.Add(1));
            nodes.Add(list.Add(2));
            nodes.Add(list.Add(3));
            nodes.Add(list.Add(4));
            nodes.Add(list.Add(5));
            nodes.Add(list.Add(6));
            Assert.AreEqual(list.AllocatedNodeCount, 6);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            nodes.Add(list.Add(7));
            nodes.Add(list.Add(8));
            Assert.AreEqual(list.AllocatedNodeCount, 7);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            int flip = list.Count - 1 - index;
            list.Remove(nodes[flip]);
            nodes.RemoveAt(flip);

            nodes.Add(list.Add(7));
            nodes.Add(list.Add(8));
            nodes.Add(list.Add(9));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            list.Remove(nodes[flip]);
            nodes.RemoveAt(flip);
            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            nodes.Add(list.Add(10));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            nodes.Add(list.Add(10));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            list.Remove(nodes[0]);
            nodes.RemoveAt(0);

            nodes.Add(list.Add(11));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[nodes.Count - 1]);
            nodes.RemoveAt(nodes.Count - 1);
            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            nodes.Add(list.Add(12));

            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            LinkedListLite<int>.Node current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }
        }

        [Test]
        public void TestClear0()
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            list.Clear();
            Assert.AreEqual(list.Count, 0);

            list.Add(1);
            Assert.AreEqual(list.AllocatedNodeCount, 1);
            Assert.AreEqual(list.Count, 1);
            Assert.AreEqual(list.Head.value, 1);

            list.Clear();
            Assert.AreEqual(list.Count, 0);
            Assert.Null(list.Head);
        }

        [Test]
        public void TestClear1([ValueSource(nameof(indexToRemove))] int index)
        {
            LinkedListLite<int> list = new LinkedListLite<int>();
            List<LinkedListLite<int>.Node> nodes = new List<LinkedListLite<int>.Node>();

            nodes.Add(list.Add(1));
            nodes.Add(list.Add(2));
            nodes.Add(list.Add(3));
            nodes.Add(list.Add(4));
            nodes.Add(list.Add(5));
            nodes.Add(list.Add(6));
            Assert.AreEqual(list.AllocatedNodeCount, 6);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            nodes.Add(list.Add(7));
            nodes.Add(list.Add(8));
            Assert.AreEqual(list.AllocatedNodeCount, 7);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            int flip = list.Count - 1 - index;
            list.Remove(nodes[flip]);
            nodes.RemoveAt(flip);

            nodes.Add(list.Add(7));
            nodes.Add(list.Add(8));
            nodes.Add(list.Add(9));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            list.Remove(nodes[flip]);
            nodes.RemoveAt(flip);
            list.Remove(nodes[index]);
            nodes.RemoveAt(index);

            list.Clear();
            nodes.Clear();
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            nodes.Add(list.Add(10));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[0]);
            nodes.RemoveAt(0);

            nodes.Add(list.Add(10));
            nodes.Add(list.Add(11));
            nodes.Add(list.Add(12));
            nodes.Add(list.Add(13));
            nodes.Add(list.Add(14));
            nodes.Add(list.Add(15));
            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            list.Remove(nodes[index]);
            nodes.RemoveAt(index);
            list.Remove(nodes[0]);
            nodes.RemoveAt(0);

            list.Remove(nodes[nodes.Count - 1]);
            nodes.RemoveAt(nodes.Count - 1);
            list.Remove(nodes[0]);
            nodes.RemoveAt(0);

            nodes.Add(list.Add(12));

            Assert.AreEqual(list.AllocatedNodeCount, 8);
            Assert.AreEqual(list.Count, nodes.Count);

            LinkedListLite<int>.Node current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }
        }

        [Test]
        public void TestAddExtend1()
        {
            LinkedListLite<int> list = new LinkedListLite<int>(2);
            list.Add(0);
            Assert.AreEqual(list.AllocatedNodeCount, 1);
            Assert.AreEqual(list.Capacity, 2);
            list.Add(1);
            Assert.AreEqual(list.AllocatedNodeCount, 2);
            Assert.AreEqual(list.Capacity, 2);
            list.Add(2);
            Assert.AreEqual(list.AllocatedNodeCount, 3);
            Assert.AreEqual(list.Capacity, 4);
            list.Add(3);
            Assert.AreEqual(list.AllocatedNodeCount, 4);
            Assert.AreEqual(list.Capacity, 4);
        }

        private static int[] capacities = new int[] { 0, 1, 2, 3, 32 };

        [Test]
        public void TestAddExtend2([ValueSource(nameof(capacities))] int capacity)
        {
            LinkedListLite<int> list = new LinkedListLite<int>(capacity);
            List<LinkedListLite<int>.Node> nodes = new List<LinkedListLite<int>.Node>();

            for (int i = 0; i < capacity; ++i)
            {
                nodes.Add(list.Add(i));
            }

            Assert.AreEqual(list.AllocatedNodeCount, capacity);
            Assert.AreEqual(list.Count, capacity);
            Assert.AreEqual(list.Capacity, capacity);

            nodes.Add(list.Add(0));
            int newCapacity = Math.Max(capacity * 2, 1);

            Assert.AreEqual(list.AllocatedNodeCount, capacity + 1);
            Assert.AreEqual(list.Count, capacity + 1);
            Assert.AreEqual(list.Capacity, newCapacity);

            int toAdd = newCapacity - list.Count;
            for (int i = 0; i < toAdd; ++i)
            {
                nodes.Add(list.Add(i));
            }

            Assert.AreEqual(list.AllocatedNodeCount, newCapacity);
            Assert.AreEqual(list.Count, newCapacity);
            Assert.AreEqual(list.Capacity, newCapacity);

            list.Remove(nodes[0]);
            nodes.RemoveAt(0);

            Assert.AreEqual(list.AllocatedNodeCount, newCapacity);
            Assert.AreEqual(list.Count, newCapacity - 1);
            Assert.AreEqual(list.Capacity, newCapacity);

            nodes.Add(list.Add(0));
            nodes.Add(list.Add(0));
            nodes.Add(list.Add(0));

            list.Remove(nodes[0]);
            nodes.RemoveAt(0);
            int last = list.Count - 1;
            list.Remove(nodes[last]);
            nodes.RemoveAt(last);

            LinkedListLite<int>.Node current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }
        }

        [Test]
        public void TestShrink1()
        {
            LinkedListLite<int> list = new LinkedListLite<int>(0);
            var n0 = list.Add(0);
            var n1 = list.Add(0);
            var n2 = list.Add(0);
            var n3 = list.Add(0);
            var n4 = list.Add(0);
            Assert.AreEqual(list.Capacity, 8);
            list.Remove(n0);
            list.Remove(n1);
            list.Remove(n2);
            list.Remove(n3);

            list.Shrink();

            Assert.AreEqual(list.Capacity, 1);
        }

        [Test]
        public void TestShrink2([ValueSource(nameof(capacities))] int capacity)
        {
            LinkedListLite<int> list = new LinkedListLite<int>(capacity);
            List<LinkedListLite<int>.Node> nodes = new List<LinkedListLite<int>.Node>();

            for (int i = 0; i < capacity; ++i)
            {
                nodes.Add(list.Add(i));
            }

            Assert.AreEqual(list.AllocatedNodeCount, capacity);
            Assert.AreEqual(list.Count, capacity);
            Assert.AreEqual(list.Capacity, capacity);

            nodes.Add(list.Add(0));
            int newCapacity = Math.Max(capacity * 2, 1);

            Assert.AreEqual(list.AllocatedNodeCount, capacity + 1);
            Assert.AreEqual(list.Count, capacity + 1);
            Assert.AreEqual(list.Capacity, newCapacity);

            list.Clear();
            nodes.Clear();

            Assert.AreEqual(list.AllocatedNodeCount, capacity + 1);
            Assert.AreEqual(list.Count, 0);
            Assert.AreEqual(list.Capacity, newCapacity);

            list.Shrink();

            Assert.AreEqual(list.AllocatedNodeCount, list.Capacity);
            Assert.AreEqual(list.Count, 0);
            Assert.AreEqual(list.Capacity, capacity);

            int toAdd = newCapacity - list.Count;
            for (int i = 0; i < toAdd; ++i)
            {
                nodes.Add(list.Add(i));
            }

            Assert.AreEqual(list.AllocatedNodeCount, newCapacity);
            Assert.AreEqual(list.Count, newCapacity);
            Assert.AreEqual(list.Capacity, newCapacity);

            if (list.Count > 0)
            {
                list.Remove(nodes[0]);
                nodes.RemoveAt(0);

                Assert.AreEqual(list.AllocatedNodeCount, newCapacity);
                Assert.AreEqual(list.Count, newCapacity - 1);
                Assert.AreEqual(list.Capacity, newCapacity);
            }

            for (int i = 0; i < 22; ++i)
            {
                nodes.Add(list.Add(i * 10));
            }

            Assert.AreEqual(list.Count, nodes.Count);

            LinkedListLite<int>.Node current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }

            for (int i = 0; i < 19; ++i)
            {
                int index = 0;
                if (i % 2 == 0)
                {
                    index = list.Count - 1;
                }

                list.Remove(nodes[index]);
                nodes.RemoveAt(index);
            }

            Assert.AreEqual(list.Count, nodes.Count);

            current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }

            list.Shrink();

            list.Remove(nodes[0]);
            nodes.RemoveAt(0);
            int last = list.Count - 1;
            list.Remove(nodes[last]);
            nodes.RemoveAt(last);

            list.Shrink();

            Assert.AreEqual(list.Count, nodes.Count);

            current = list.Head;
            for (int i = 0; i < nodes.Count; ++i)
            {
                Assert.AreEqual(nodes[i], current);
                current = list.Next(current);
            }
        }
    }
}

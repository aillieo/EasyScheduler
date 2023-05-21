// -----------------------------------------------------------------------
// <copyright file="LinkedListLite.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine.Assertions;

    internal class LinkedListLite<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private Node[] buffer;

        private int head = -1;
        private int free = 0;
        private int count;

        private int capacity = 8;

        public class Node
        {
            public T value;

            public int index;

            public int next;
            public int previous;

            public override string ToString()
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                return $"Node<value={this.value}(index={this.index},next={this.next},previous={this.previous})>";
#else
                return base.ToString();
#endif
            }
        }

        public LinkedListLite(int capacity = 8)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("Capacity less than zero", nameof(capacity));
            }

            this.capacity = capacity;
            this.buffer = new Node[capacity];
        }

        public int Capacity
        {
            get
            {
                return this.buffer.Length;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public Node Head
        {
            get
            {
                if (this.head >= 0)
                {
                    return this.buffer[this.head];
                }

                return null;
            }
        }

        public int AllocatedNodeCount
        {
            get
            {
                for (int i = 0; i < this.buffer.Length; ++i)
                {
                    if (this.buffer[i] == null)
                    {
                        return i;
                    }
                }

                return this.buffer.Length;
            }
        }

        public bool IsReadOnly => throw new NotImplementedException();

        public Node Add(T item)
        {
            Node newNode = this.GetNewNode();

            if (this.head == -1)
            {
                this.head = newNode.index;
                newNode.next = this.head;
                newNode.previous = this.head;
            }
            else
            {
                newNode.next = this.head;
                newNode.previous = this.Head.previous;
                this.buffer[this.Head.previous].next = newNode.index;
                this.Head.previous = newNode.index;
            }

            newNode.value = item;

            this.count++;

            return newNode;
        }

        public bool Remove(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (node.index < 0)
            {
                return false;
            }

            if (this.count == 0)
            {
                return false;
            }

            if (node.index == node.next)
            {
                Assert.AreEqual(this.count, 1);

                this.head = -1;
            }
            else
            {
                bool removeHead = node.index == this.head;

                int previous = node.previous;
                int next = node.next;
                Node previousNode = this.buffer[previous];
                Node nextNode = this.buffer[next];
                previousNode.next = next;
                nextNode.previous = previous;

                if (removeHead)
                {
                    this.head = node.next;
                }
            }

            this.count--;

            this.RecycleNode(node);

            return true;
        }

        public void Clear()
        {
            if (this.count == 0)
            {
                return;
            }

            if (this.Head.next == this.head)
            {
                this.RecycleNode(this.Head);
                this.head = -1;
                this.count = 0;
            }
            else
            {
                int tail = this.Head.previous;
                Node tailNode = this.buffer[tail];
                tailNode.next = this.free;
                this.free = this.head;
                this.head = -1;

                this.count = 0;
            }
        }

        public Node Next(Node current)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            int next = current.next;

            if (next < 0)
            {
                throw new ArgumentException($"Invalid node, index={current.index}", nameof(current));
            }

            if (next == this.head)
            {
                return null;
            }

            return this.buffer[next];
        }

        private Node GetNewNode()
        {
            if (this.free >= this.buffer.Length)
            {
                this.Grow();
            }

            Assert.IsTrue(this.free >= 0 && this.free < this.buffer.Length);

            Node newNode = this.buffer[this.free];

            if (newNode == null)
            {
                newNode = new Node()
                {
                    index = this.free,
                    previous = -1,
                    next = -1,
                };

                this.buffer[this.free] = newNode;
                this.free++;
            }
            else
            {
                int next = newNode.next;
                newNode.next = -1;
                newNode.previous = -1;
                this.free = next;
            }

            return newNode;
        }

        private void RecycleNode(Node node)
        {
            if (node.index < 0 || node.index >= this.buffer.Length)
            {
                throw new IndexOutOfRangeException();
            }

            Assert.AreEqual(node, this.buffer[node.index]);
            node.next = this.free;
            this.free = node.index;
        }

        private void Grow()
        {
            var length = this.buffer.Length;
            Array.Resize(ref this.buffer, Math.Max(length * 2, 1));
            this.free = length;
        }

        public void Shrink()
        {
            if (this.buffer.Length <= this.capacity || this.buffer.Length < this.count * 2)
            {
                return;
            }

            int newLength = capacity;
            while (newLength < this.count)
            {
                if (newLength == 0)
                {
                    newLength = 1;
                }
                else
                {
                    newLength = newLength << 1;
                }
            }

            Assert.IsTrue(newLength < buffer.Length);

            Node[] newBuffer = new Node[newLength];
            Node current = this.Head;

            int index = 0;

            int newHead = -1;
            if (count > 0)
            {
                newHead = 0;
            }

            while (current != null)
            {
                newBuffer[index] = current;
                index++;
                current = Next(current);
            }

            int oldFree = this.free;
            int toInsert = this.count;
            while (oldFree < buffer.Length && toInsert < newBuffer.Length)
            {
                Node freeNode = buffer[oldFree];

                if (freeNode == null)
                {
                    break;
                }

                newBuffer[toInsert] = freeNode;
                oldFree = freeNode.next;
                toInsert++;
            }

            for (int i = 0; i < count; ++i)
            {
                Node node = newBuffer[i];
                node.index = i;
                node.previous = (i + this.count - 1) % this.count;
                node.next = (i + 1) % this.count;
            }

            for (int i = count; i < newBuffer.Length; ++i)
            {
                Node node = newBuffer[i];
                if (node == null)
                {
                    break;
                }

                node.index = i;
                node.previous = i - 1;
                node.next = i + 1;
            }

            this.buffer = newBuffer;
            this.head = newHead;
            this.free = this.count;
        }

        public void DebugPrint()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');

            Node node = this.Head;
            while (node != null)
            {
                stringBuilder.Append(node.value);
                stringBuilder.Append(',');
                node = this.Next(node);
            }

            stringBuilder.Append(']');
            UnityEngine.Debug.Log(stringBuilder);
        }

        public void DebugPrint2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"head={head} free={free} count={count}");

            for (int i = 0; i < buffer.Length; i ++)
            {
                Node node = buffer[i];
                if (node != null)
                {
                    stringBuilder.Append($"{node.value} (index={node.index},prev={node.previous},next={node.next})");

                    if (i == head)
                    {
                        stringBuilder.Append(" [H]");
                    }

                    if (i == free)
                    {
                        stringBuilder.Append(" [F]");
                    }

                    stringBuilder.AppendLine();
                }
                else
                {
                    stringBuilder.AppendLine("Null");
                }
            }

            UnityEngine.Debug.Log(stringBuilder);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int current = head;

            if (current >= 0)
            {
                do
                {
                    Node node = buffer[current];
                    array[arrayIndex++] = node.value;
                    current = node.next;
                }

                while (current != head);
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            int current = head;
            var removeCount = 0;

            if (current >= 0)
            {
                do
                {
                    Node node = buffer[current];

                    if (match(node.value))
                    {
                        var next = node.next;
                        Remove(node);
                        current = next;

                        removeCount++;
                    }
                    else
                    {
                        current = node.next;
                    }
                }

                while (current != head);
            }

            return removeCount;
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        internal sealed class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly LinkedListLite<T> list;
            private LinkedListLite<T>.Node node;

            internal Enumerator(LinkedListLite<T> list)
            {
                this.list = list;
                this.node = list.Head;
            }

            public T Current => node.value;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (node == null) { return false; }
                node = list.Next(node);
                return list.Next(node) != null;
            }

            public void Reset()
            {
            }
        }
    }
}

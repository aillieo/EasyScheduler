// -----------------------------------------------------------------------
// <copyright file="LinkedListExtensions.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;

    internal static class LinkedListExtensions
    {
        public static int RemoveAll<T>(this LinkedList<T> linkedList, Predicate<T> match)
        {
            if (linkedList == null)
            {
                throw new ArgumentNullException(nameof(linkedList));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (linkedList.Count == 0)
            {
                return 0;
            }

            LinkedListNode<T> current = linkedList.First;
            var removeCount = 0;

            while (current != null)
            {
                if (match(current.Value))
                {
                    LinkedListNode<T> next = current.Next;
                    current.List.Remove(current);
                    current = next;

                    removeCount++;
                }
                else
                {
                    current = current.Next;
                }
            }

            return removeCount;
        }
    }
}

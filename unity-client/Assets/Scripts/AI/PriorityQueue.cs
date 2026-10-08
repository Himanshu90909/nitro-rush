using System;
using System.Collections.Generic;

namespace NitroRush.AI
{
    /// <summary>
    /// Generic binary min-heap priority queue implementation.
    /// Time Complexities:
    /// - Push: O(log n) - standard binary heap sift-up.
    /// - Pop: O(log n) - root removal with sift-down.
    /// - Peek: O(1) - constant time inspection of the top element.
    /// - Count: O(1) - property access to inner buffer length.
    /// </summary>
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _heap = new List<(TElement, TPriority)>();

        public int Count => _heap.Count;

        /// <summary>
        /// Inserts an item into the min-heap with priority. O(log n).
        /// </summary>
        public void Push(TElement element, TPriority priority)
        {
            _heap.Add((element, priority));
            SiftUp(_heap.Count - 1);
        }

        /// <summary>
        /// Removes and returns the item with lowest priority value. O(log n).
        /// </summary>
        public TElement Pop()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Priority queue is empty.");

            TElement result = _heap[0].Element;
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            if (_heap.Count > 0)
            {
                SiftDown(0);
            }

            return result;
        }

        /// <summary>
        /// Inspects the top item without removing it. O(1).
        /// </summary>
        public TElement Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Priority queue is empty.");
            return _heap[0].Element;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_heap[index].Priority.CompareTo(_heap[parentIndex].Priority) >= 0)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void SiftDown(int index)
        {
            int lastIndex = _heap.Count - 1;
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if (leftChild <= lastIndex && _heap[leftChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
                {
                    smallest = leftChild;
                }

                if (rightChild <= lastIndex && _heap[rightChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
                {
                    smallest = rightChild;
                }

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;
        }
    }
}

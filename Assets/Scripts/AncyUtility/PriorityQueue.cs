using System;
using System.Collections.Generic;

namespace AncyUtility
{
    public class PriorityQueue<T>
    {
        private readonly List<T> heap;
        private readonly Func<T, T, float> comparer;

        public int Count => heap.Count;

        public PriorityQueue(Func<T, T, float> comparer)
        {
            this.heap = new List<T>();
            this.comparer = comparer;
        }

        public void Enqueue(T item)
        {
            heap.Add(item);
            var i = Count - 1;
            while (i > 0)
            {
                var parent = (i - 1) / 2;
                if (comparer(heap[parent], heap[i]) <= 0)
                    break;
                Swap(parent, i);
                i = parent;
            }
        }

        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("Priority queue is empty");
            var min = heap[0];
            heap[0] = heap[Count - 1];
            heap.RemoveAt(Count - 1);
            Heapify(0);
            return min;
        }

        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("Priority queue is empty");
            return heap[0];
        }

        public bool Remove(T item)
        {
            int index = heap.IndexOf(item);
            if (index < 0)
                return false;
            heap[index] = heap[Count - 1];
            heap.RemoveAt(Count - 1);
            Heapify(index);
            return true;
        }

        public bool Contains(T item)
        {
            return heap.Contains(item);
        }

        private void Heapify(int i)
        {
            while (true)
            {
                var left = 2 * i + 1;
                var right = 2 * i + 2;
                var smallest = i;
                if (left < Count && comparer(heap[left], heap[smallest]) < 0) smallest = left;
                if (right < Count && comparer(heap[right], heap[smallest]) < 0) smallest = right;
                if (smallest != i)
                {
                    Swap(i, smallest);
                    i = smallest;
                    continue;
                }

                break;
            }
        }

        private void Swap(int i, int j)
        {
            (heap[i], heap[j]) = (heap[j], heap[i]);
        }

        public void Clear()
        {
            heap.Clear();
        }
    }
}
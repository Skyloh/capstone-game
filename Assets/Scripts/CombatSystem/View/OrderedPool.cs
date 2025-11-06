using System;
using System.Collections.Generic;

namespace CombatSystem.View
{
    public class OrderedPool<T>
    {
        private readonly T[] array;

        private int activeLength = 0;

        public OrderedPool(int capacity)
        {
            array = new T[capacity];
        }

        public T this[Index i]
        {
            get
            {
                if (i.IsFromEnd)
                {
                    return array[activeLength - i.Value];
                }

                if (i.Value >= activeLength)
                {
                    throw new IndexOutOfRangeException(
                        $"Index: {i.Value} not within active range of pool {activeLength}");
                }

                return array[i.Value];
            }
            set
            {
                if (i.IsFromEnd)
                {
                    if (i.Value == 0)
                    {
                        throw new IndexOutOfRangeException("Out side of active range of pool");
                    }

                    array[activeLength - i.Value] = value;
                }

                if (i.Value >= activeLength)
                {
                    throw new IndexOutOfRangeException("Out side of active range of pool");
                }

                array[i.Value] = value;
            }
        }

        public void Initialize(int index, T obj)
        {
            array[index] = obj;
        }

        public IEnumerable<T> ActivePool()
        {
            for (int i = 0; i < activeLength; i++)
            {
                yield return array[i];
            }
        }

        public IEnumerable<T> EntirePool()
        {
            foreach (var t in array)
            {
                yield return t;
            }
        }

        public int GetActiveCount()
        {
            return activeLength;
        }

        public int PoolSize => array.Length;

        public void AddToBack(Action<T> action)
        {
            action(array[activeLength]);
            activeLength++;
        }

        public void RemoveFromBack(Action<T> action)
        {
            action(array[activeLength - 1]);
            activeLength--;
        }

        public void Clear(Action<T> action)
        {
            for (int i = activeLength - 1; i >= 0; i--)
            {
               action(array[i]); 
            }
            activeLength = 0;
        }
    }
}
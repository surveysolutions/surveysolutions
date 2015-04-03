using System;

namespace WB.Core.Infrastructure.EventBus.Implementation
{
    internal class ImmutableList<T>
    {
        private readonly T[] data;

        public ImmutableList()
        {
            this.data = new T[0];
        }

        private ImmutableList(T[] data)
        {
            this.data = data;
        }

        public ImmutableList<T> Add(T value)
        {
            T[] destinationArray = new T[data.Length + 1];
            Array.Copy(data, destinationArray, data.Length);
            destinationArray[data.Length] = value;
            return new ImmutableList<T>(destinationArray);
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public ImmutableList<T> Remove(T value)
        {
            int index = IndexOf(value);
            if (index < 0)
                return this;

            T[] destinationArray = new T[data.Length - 1];
            Array.Copy(data, 0, destinationArray, 0, index);
            Array.Copy(data, index + 1, destinationArray, index, (data.Length - index) - 1);
            return new ImmutableList<T>(destinationArray);
        }

        public int Count
        {
            get { return data.Length; }
        }

        public T[] Data
        {
            get { return data; }
        }

        public T this[int index]
        {
            get { return data[index]; }
        }
    }
}
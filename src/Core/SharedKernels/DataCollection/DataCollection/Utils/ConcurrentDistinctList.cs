using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public class ConcurrentDistinctList<T> : IEnumerable<T>
    {
        private readonly object lockObject = new object();
        private readonly List<T> list = new List<T>();

        public ConcurrentDistinctList() { }

        public ConcurrentDistinctList(IEnumerable<T> decimals)
            : this()
        {
            this.list = new List<T>(decimals.Distinct());
        }

        public void Add(T value)
        {
            lock (this.lockObject)
            {
                if (!this.list.Contains(value))
                {
                    this.list.Add(value);
                }
            }
        }
        
        public void Remove(T value)
        {
            lock (this.lockObject)
            {
                this.list.Remove(value);
            }
        }

        public bool Contains(T value)
        {
            lock (this.lockObject)
            {
                return this.list.Contains(value);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this.lockObject)
            {
                return this.list.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.lockObject)
            {
                return ((IEnumerable) this.list).GetEnumerator();
            }
        }
    }
}
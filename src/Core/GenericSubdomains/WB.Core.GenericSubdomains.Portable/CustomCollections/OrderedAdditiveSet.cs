using System.Collections;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class OrderedAdditiveSet<T> : IEnumerable<T>
    {
        private readonly HashSet<T> checker;
        private readonly List<T> values;

        public OrderedAdditiveSet(IEqualityComparer<T> comparer)
        {
            this.checker = new HashSet<T>(comparer);
            this.values = new List<T>();
        }

        public bool Add(T item)
        {
            if (this.checker.Contains(item)) return false;
            this.values.Add(item);
            this.checker.Add(item);
            return true;
        }

        public bool Contains(T item)
        {
            return this.checker.Contains(item);
        }

        public int Count => this.checker.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return this.values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
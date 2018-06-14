using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable.CustomCollections
{
    public class ConcurrentHashSet<T> : ICollection<T>, IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, byte> dictionary;

        public ConcurrentHashSet()
        {
            this.dictionary = new ConcurrentDictionary<T,byte>();
        }

        public ConcurrentHashSet(IEnumerable<T> collection)
        {
            var keyValueCollection = collection.Select(x => new KeyValuePair<T, byte>(x, 0));
            this.dictionary = new ConcurrentDictionary<T, byte>(keyValueCollection);
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            this.dictionary = new ConcurrentDictionary<T, byte>(comparer);
        }

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            var keyValueCollection = collection.Select(x => new KeyValuePair<T, byte>(x, 0));
            this.dictionary = new ConcurrentDictionary<T, byte>(keyValueCollection, comparer);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.dictionary.Keys.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            byte removedValue;
            return this.dictionary.TryRemove(item, out removedValue);
        }

        public int Count => this.dictionary.Count;
        public bool IsReadOnly => false;

        public void Remove(T item)
        {
            this.dictionary.TryRemove(item, out _);
        }

        public void Add(T item)
        {
            this.dictionary.TryAdd(item, 0);
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return this.dictionary.ContainsKey(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.dictionary.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

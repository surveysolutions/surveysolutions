﻿using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public class ConcurrentHashSet<T> : ICollection<T>, IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, byte> dictionary;

        public ConcurrentHashSet()
        {
            var a = new HashSet<T>();
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
            dictionary.Keys.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            byte removedVelue;
            return dictionary.TryRemove(item, out removedVelue);
        }

        public int Count { get { return dictionary.Count; } }
        public bool IsReadOnly { get {return false; } }

        public void Remove(T item)
        {
            this.dictionary.Remove(item);
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

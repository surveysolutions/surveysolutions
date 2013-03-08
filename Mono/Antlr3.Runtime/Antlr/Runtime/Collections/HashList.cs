namespace Antlr.Runtime.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    public sealed class HashList : IDictionary, ICollection, IEnumerable
    {
        private Hashtable _dictionary;
        private List<object> _insertionOrderList;
        private int _version;

        public HashList() : this(-1)
        {
        }

        public HashList(int capacity)
        {
            this._dictionary = new Hashtable();
            this._insertionOrderList = new List<object>();
            if (capacity < 0)
            {
                this._dictionary = new Hashtable();
                this._insertionOrderList = new List<object>();
            }
            else
            {
                this._dictionary = new Hashtable(capacity);
                this._insertionOrderList = new List<object>(capacity);
            }
            this._version = 0;
        }

        public void Add(object key, object value)
        {
            this._dictionary.Add(key, value);
            this._insertionOrderList.Add(key);
            this._version++;
        }

        public void Clear()
        {
            this._dictionary.Clear();
            this._insertionOrderList.Clear();
            this._version++;
        }

        public bool Contains(object key)
        {
            return this._dictionary.Contains(key);
        }

        private void CopyKeysTo(Array array, int index)
        {
            int count = this._insertionOrderList.Count;
            for (int i = 0; i < count; i++)
            {
                array.SetValue(this._insertionOrderList[i], index++);
            }
        }

        public void CopyTo(Array array, int index)
        {
            int count = this._insertionOrderList.Count;
            for (int i = 0; i < count; i++)
            {
                DictionaryEntry entry = new DictionaryEntry(this._insertionOrderList[i], this._dictionary[this._insertionOrderList[i]]);
                array.SetValue(entry, index++);
            }
        }

        private void CopyValuesTo(Array array, int index)
        {
            int count = this._insertionOrderList.Count;
            for (int i = 0; i < count; i++)
            {
                array.SetValue(this._dictionary[this._insertionOrderList[i]], index++);
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new HashListEnumerator(this, HashListEnumerator.EnumerationMode.Entry);
        }

        public void Remove(object key)
        {
            this._dictionary.Remove(key);
            this._insertionOrderList.Remove(key);
            this._version++;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new HashListEnumerator(this, HashListEnumerator.EnumerationMode.Entry);
        }

        public int Count
        {
            get
            {
                return this._dictionary.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return this._dictionary.IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this._dictionary.IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this._dictionary.IsSynchronized;
            }
        }

        public object this[object key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                bool flag = !this._dictionary.Contains(key);
                this._dictionary[key] = value;
                if (flag)
                {
                    this._insertionOrderList.Add(key);
                }
                this._version++;
            }
        }

        public ICollection Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        public object SyncRoot
        {
            get
            {
                return this._dictionary.SyncRoot;
            }
        }

        public ICollection Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        private sealed class HashListEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private HashList _hashList;
            private int _index;
            private object _key;
            private EnumerationMode _mode;
            private List<object> _orderList;
            private object _value;
            private int _version;

            internal HashListEnumerator()
            {
                this._index = 0;
                this._key = null;
                this._value = null;
            }

            internal HashListEnumerator(HashList hashList, EnumerationMode mode)
            {
                this._hashList = hashList;
                this._mode = mode;
                this._version = hashList._version;
                this._orderList = hashList._insertionOrderList;
                this._index = 0;
                this._key = null;
                this._value = null;
            }

            public bool MoveNext()
            {
                if (this._version != this._hashList._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                if (this._index < this._orderList.Count)
                {
                    this._key = this._orderList[this._index];
                    this._value = this._hashList[this._key];
                    this._index++;
                    return true;
                }
                this._key = null;
                return false;
            }

            public void Reset()
            {
                if (this._version != this._hashList._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                this._index = 0;
                this._key = null;
                this._value = null;
            }

            public object Current
            {
                get
                {
                    if (this._key == null)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    if (this._mode == EnumerationMode.Key)
                    {
                        return this._key;
                    }
                    if (this._mode == EnumerationMode.Value)
                    {
                        return this._value;
                    }
                    return new DictionaryEntry(this._key, this._value);
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if (this._key == null)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return new DictionaryEntry(this._key, this._value);
                }
            }

            public object Key
            {
                get
                {
                    if (this._key == null)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return this._key;
                }
            }

            public object Value
            {
                get
                {
                    if (this._key == null)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return this._value;
                }
            }

            internal enum EnumerationMode
            {
                Key,
                Value,
                Entry
            }
        }

        private sealed class KeyCollection : ICollection, IEnumerable
        {
            private HashList _hashList;

            internal KeyCollection()
            {
            }

            internal KeyCollection(HashList hashList)
            {
                this._hashList = hashList;
            }

            public void CopyTo(Array array, int index)
            {
                this._hashList.CopyKeysTo(array, index);
            }

            public override bool Equals(object o)
            {
                if (o is HashList.KeyCollection)
                {
                    HashList.KeyCollection keys = (HashList.KeyCollection) o;
                    if ((this.Count == 0) && (keys.Count == 0))
                    {
                        return true;
                    }
                    if (this.Count == keys.Count)
                    {
                        for (int i = 0; i < this.Count; i++)
                        {
                            if ((this._hashList._insertionOrderList[i] == keys._hashList._insertionOrderList[i]) || this._hashList._insertionOrderList[i].Equals(keys._hashList._insertionOrderList[i]))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public IEnumerator GetEnumerator()
            {
                return new HashList.HashListEnumerator(this._hashList, HashList.HashListEnumerator.EnumerationMode.Key);
            }

            public override int GetHashCode()
            {
                return this._hashList._insertionOrderList.GetHashCode();
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[");
                List<object> list = this._hashList._insertionOrderList;
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(list[i]);
                }
                builder.Append("]");
                return builder.ToString();
            }

            public int Count
            {
                get
                {
                    return this._hashList.Count;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return this._hashList.IsSynchronized;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return this._hashList.SyncRoot;
                }
            }
        }

        private sealed class ValueCollection : ICollection, IEnumerable
        {
            private HashList _hashList;

            internal ValueCollection()
            {
            }

            internal ValueCollection(HashList hashList)
            {
                this._hashList = hashList;
            }

            public void CopyTo(Array array, int index)
            {
                this._hashList.CopyValuesTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return new HashList.HashListEnumerator(this._hashList, HashList.HashListEnumerator.EnumerationMode.Value);
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[");
                IEnumerator enumerator = new HashList.HashListEnumerator(this._hashList, HashList.HashListEnumerator.EnumerationMode.Value);
                if (enumerator.MoveNext())
                {
                    builder.Append((enumerator.Current == null) ? "null" : enumerator.Current);
                    while (enumerator.MoveNext())
                    {
                        builder.Append(", ");
                        builder.Append((enumerator.Current == null) ? "null" : enumerator.Current);
                    }
                }
                builder.Append("]");
                return builder.ToString();
            }

            public int Count
            {
                get
                {
                    return this._hashList.Count;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return this._hashList.IsSynchronized;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return this._hashList.SyncRoot;
                }
            }
        }
    }
}


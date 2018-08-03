using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    public class SynchronizedList<T> : IDisposable, IList<T>, IList, IReadOnlyList<T>
    {
        #region Constructor
        public SynchronizedList()
        {
            this._items = new List<T>();
        }

        public SynchronizedList(IEnumerable<T> items)
        {
            this._items = items.ToList();
        }
        #endregion

        #region Private Fields
        private readonly IList<T> _items;
        private readonly ReaderWriterLockSlim _itemsLock = new ReaderWriterLockSlim();
        private Object _syncRoot;
        #endregion
        
        #region Private Properties
        bool IList.IsFixedSize
        {
            get
            {
                //TODO: Do I need a lock here?
                // There is no IList<T>.IsFixedSize, so we must assume that only
                // readonly collections are fixed size, if our internal item 
                // collection does not implement IList.  Note that Array implements
                // IList, and therefore T[] and U[] will be fixed-size.
                var list = this._items as IList;
                if (list != null)
                {
                    return list.IsFixedSize;
                }

                return this._items.IsReadOnly;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return this._items.IsReadOnly; }
        }

        bool IList.IsReadOnly
        {
            get { return this._items.IsReadOnly; }
        }

        //TODO: Does this mean what I think it does?
        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    this._itemsLock.EnterReadLock();

                    try
                    {
                        var c = this._items as ICollection;
                        if (c != null)
                        {
                            this._syncRoot = c.SyncRoot;
                        }
                        else
                        {
                            Interlocked.CompareExchange<Object>(ref this._syncRoot, new Object(), null);
                        }
                    }
                    finally
                    {
                        this._itemsLock.ExitReadLock();
                    }
                }

                return this._syncRoot;
            }
        }
        #endregion

        #region Public Properties
        public int Count
        {
            get
            {
                this._itemsLock.EnterReadLock();

                try
                {
                    return this._items.Count;
                }
                finally
                {
                    this._itemsLock.ExitReadLock();
                }
            }
        }

        public T this[int index]
        {
            get
            {
                this._itemsLock.EnterReadLock();

                try
                {
                    this.CheckIndex(index);
                    return this._items[index];
                }
                finally
                {
                    this._itemsLock.ExitReadLock();
                }
            }
            set
            {
                this._itemsLock.EnterWriteLock();

                try
                {
                    this.CheckIsReadOnly();
                    this.CheckIndex(index);

                    this._items[index] = value;

                }
                finally
                {
                    this._itemsLock.ExitWriteLock();
                }
            }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("'value' is the wrong type");
                }
            }
        }

        #endregion

        #region Private Methods

        // ReSharper disable once UnusedParameter.Local
        private void CheckIndex(int index)
        {
            if (index < 0 || index > this._items.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckIsReadOnly()
        {
            if (this._items.IsReadOnly)
            {
                throw new NotSupportedException("Collection is readonly");
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }
        #endregion

        #region Public Methods
        public void Add(T item)
        {
            this._itemsLock.EnterWriteLock();

            var index = this._items.Count;

            try
            {
                this.CheckIsReadOnly();

                this._items.Insert(index, item);
            }
            finally
            {
                this._itemsLock.ExitWriteLock();
            }
        }

        int IList.Add(object value)
        {
            this._itemsLock.EnterWriteLock();

            var index = this._items.Count;

            try
            {
                this.CheckIsReadOnly();

                var item = (T)value;

                this._items.Insert(index, item);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("'value' is the wrong type");
            }
            finally
            {
                this._itemsLock.ExitWriteLock();
            }

            return index;
        }

        public void Clear()
        {
            this._itemsLock.EnterWriteLock();

            try
            {
                this.CheckIsReadOnly();

                this._items.Clear();
            }
            finally
            {
                this._itemsLock.ExitWriteLock();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            this._itemsLock.EnterReadLock();

            try
            {
                this._items.CopyTo(array, index);
            }
            finally
            {
                this._itemsLock.ExitReadLock();
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this._itemsLock.EnterReadLock();

            try
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array", "'array' cannot be null");
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException("Multidimension arrays are not supported", "array");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Non-zero lower bound arrays are not supported", "array");
                }

                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index", "'index' is out of range");
                }

                if (array.Length - index < this._items.Count)
                {
                    throw new ArgumentException("Array is too small");
                }

                var tArray = array as T[];
                if (tArray != null)
                {
                    this._items.CopyTo(tArray, index);
                }
                else
                {
                    //
                    // Catch the obvious case assignment will fail.
                    // We can found all possible problems by doing the check though.
                    // For example, if the element type of the Array is derived from T,
                    // we can't figure out if we can successfully copy the element beforehand.
                    //
                    var targetType = array.GetType().GetElementType();
                    var sourceType = typeof(T);
                    if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }

                    //
                    // We can't cast array of value type to object[], so we don't support 
                    // widening of primitive types here.
                    //
                    var objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }

                    var count = this._items.Count;
                    try
                    {
                        for (var i = 0; i < count; i++)
                        {
                            objects[index++] = this._items[i];
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }
                }
            }
            finally
            {
                this._itemsLock.ExitReadLock();
            }
        }

        public bool Contains(T item)
        {
            this._itemsLock.EnterReadLock();

            try
            {
                return this._items.Contains(item);
            }
            finally
            {
                this._itemsLock.ExitReadLock();
            }
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                this._itemsLock.EnterReadLock();

                try
                {
                    return this._items.Contains((T)value);
                }
                finally
                {
                    this._itemsLock.ExitReadLock();
                }
            }

            return false;
        }

        public void Dispose()
        {
            this._itemsLock.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            this._itemsLock.EnterReadLock();

            try
            {
                return this._items.ToList().GetEnumerator();
            }
            finally
            {
                this._itemsLock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this._itemsLock.EnterReadLock();

            try
            {
                return ((IEnumerable)this._items.ToList()).GetEnumerator();
            }
            finally
            {
                this._itemsLock.ExitReadLock();
            }
        }

        public int IndexOf(T item)
        {
            this._itemsLock.EnterReadLock();

            try
            {
                return this._items.IndexOf(item);
            }
            finally
            {
                this._itemsLock.ExitReadLock();
            }
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                this._itemsLock.EnterReadLock();

                try
                {
                    return this._items.IndexOf((T)value);
                }
                finally
                {
                    this._itemsLock.ExitReadLock();
                }
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            this._itemsLock.EnterWriteLock();

            try
            {
                this.CheckIsReadOnly();
                this.CheckIndex(index);

                this._items.Insert(index, item);
            }
            finally
            {
                this._itemsLock.ExitWriteLock();
            }
        }

        void IList.Insert(int index, object value)
        {
            try
            {
                this.Insert(index, (T)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("'value' is the wrong type");
            }
        }

        public bool Remove(T item)
        {
            this._itemsLock.EnterUpgradeableReadLock();

            try
            {
                this.CheckIsReadOnly();

                var index = this._items.IndexOf(item);
                if (index < 0)
                {
                    return false;
                }

                this._itemsLock.EnterWriteLock();

                try
                {
                    this._items.RemoveAt(index);
                }
                finally
                {
                    this._itemsLock.ExitWriteLock();
                }
            }
            finally
            {
                this._itemsLock.ExitUpgradeableReadLock();
            }

            return true;
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                this.Remove((T)value);
            }
        }

        public void RemoveAt(int index)
        {
            this._itemsLock.EnterWriteLock();

            try
            {
                this.CheckIsReadOnly();
                this.CheckIndex(index);

                this._items.RemoveAt(index);
            }
            finally
            {
                this._itemsLock.ExitWriteLock();
            }
        }
        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    /// <remarks>
    /// This implementation supports only set of features used by us.
    /// For instance it will not notify indexer change (http://stackoverflow.com/questions/657675/propertychanged-for-indexer-property) because we do not use such bindings.
    /// The same is applied for other features.
    /// </remarks>
    [DebuggerDisplay("CompositeCollection Count={Count}")]
    public class CompositeCollection<T> : IObservableCollection<T>,
        IDisposable
    {
        private readonly ReaderWriterLockSlim itemsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Object syncRoot;

        private readonly List<IObservableCollection<T>> collections = new List<IObservableCollection<T>>();

        public bool IsReadOnly => true;

        public object this[int index]
        {
            get
            {
                this.itemsLock.EnterReadLock();
                try
                {
                    int processedItemsCount = 0;
                    for (int i = 0; i < this.collections.Count; i++)
                    {
                        var currentCollection = this.collections[i];
                        if (processedItemsCount + currentCollection.Count > index)
                        {
                            return currentCollection[index - processedItemsCount];
                        }
                        else
                        {
                            processedItemsCount += currentCollection.Count;
                        }
                    }

                    throw new IndexOutOfRangeException($" Index was outside the bounds of the array. Type argument is {typeof(T).FullName}");
                }
                finally
                {
                    this.itemsLock.ExitReadLock();
                }
            }

            set => throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            foreach (var coll in this.collections)
                foreach (var item in coll)
                    array.SetValue(item, index++);
        }

        private int count;
        public int Count {
            get => count;
            private set
            {
                count = value;
            }
         }

        public bool IsSynchronized => false;
        object ICollection.SyncRoot
        {
            get
            {
                if (this.syncRoot == null)
                {
                    this.itemsLock.EnterReadLock();

                    try
                    {
                        Interlocked.CompareExchange<Object>(ref this.syncRoot, new Object(), null);
                    }
                    finally
                    {
                        this.itemsLock.ExitReadLock();
                    }
                }

                return this.syncRoot;
            }
        }

        public int Add(object value)
        {
            this.AddCollection(new CovariantObservableCollection<T>(((T)value).ToEnumerable()));
            return this.Count;
        }

        public void Clear()
        {
            this.itemsLock.EnterWriteLock();
            try
            {
                var removedItems = this.ToList();

                this.Count = 0;

                this.collections.OfType<INotifyCollectionChanged>().ForEach(x => x.CollectionChanged -= this.HandleChildCollectionChanged);
                this.collections.Clear();

                this.NotifyItemsRemoved(removedItems, offset: 0);
            }
            finally
            {
                this.itemsLock.ExitWriteLock();
            }
        }

        public bool Contains(object value)
        {
            return this.Contains((T) value);
        }

        [ExcludeFromCodeCoverage]
        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize => false;

        public bool Contains(T item)
        {
            this.itemsLock.EnterReadLock();
            try
            {
                foreach (var coll in this.collections)
                    if (coll.Contains(item))
                        return true;
                return false;
            }
            finally
            {
                this.itemsLock.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            this.itemsLock.EnterReadLock();

            try
            {
                foreach (var coll in this.collections)
                    foreach (var item in coll ?? Enumerable.Empty<T>())
                        yield return item;
            }
            finally
            {
                this.itemsLock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            this.AddCollection(new CovariantObservableCollection<T>(item.ToEnumerable()));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddCollection(IObservableCollection<T> collection)
        {
            this.itemsLock.EnterWriteLock();

            int offset = this.Count;
            try
            {
                this.collections.Add(collection);
                collection.CollectionChanged += this.HandleChildCollectionChanged;

                var addedCollectionCount = collection.Count;
                this.Count += addedCollectionCount;
                this.NotifyItemsAdded(collection, offset);
            }
            finally
            {
                this.itemsLock.ExitWriteLock();
            }
        }

        public void InsertCollection(int index, IObservableCollection<T> collection)
        {
            this.itemsLock.EnterWriteLock();

            try
            {
                int offset = this.collections.Take(index).Sum(c => c.Count);

                this.collections.Insert(index, collection);
                collection.CollectionChanged += this.HandleChildCollectionChanged;

                var addedCollectionCount = collection.Count;
                this.Count += addedCollectionCount;

                if (addedCollectionCount > 0)
                    this.NotifyItemsAdded(collection, offset);
            }
            finally
            {
                this.itemsLock.ExitWriteLock();
            }
        }

        public bool RemoveCollection(IObservableCollection<T> collection)
        {
            this.itemsLock.EnterWriteLock();

            try
            {
                int index = this.collections.IndexOf(collection);

                if (index < 0)
                    return false;

                int offset = this.collections.Take(index).Sum(c => c.Count);

                collection.CollectionChanged -= this.HandleChildCollectionChanged;
                var removeResult = this.collections.Remove(collection);

                var collectionCount = collection.Count;
                this.Count -= collectionCount;
                
                if (collectionCount > 0)
                    this.NotifyItemsRemoved(collection, offset);

                return removeResult;
            }
            finally
            {
                this.itemsLock.ExitWriteLock();
            }
        }

        private void HandleChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int newCount = 0;
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                using (var enumerator = this.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        newCount++;
                    }
                }
            }
            else
            {
                newCount = this.Count + (e.NewItems?.Count ?? 0) - (e.OldItems?.Count ?? 0);
            }

            if (newCount != this.Count)
            {
                this.Count = newCount;
                this.OnPropertyChanged(nameof(this.Count));
            }

            if (this.CollectionChanged == null)
                return;

            var offset = CalculateOffset(sender);
            var newIndex = e.NewStartingIndex == -1 ? -1 : e.NewStartingIndex + offset;
            var oldIndex = e.OldStartingIndex == -1 ? -1 : e.OldStartingIndex + offset;

            // This must be the most unfriendly set of event args in the entire .NET framework...
            NotifyCollectionChangedEventArgs args;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
                case NotifyCollectionChangedAction.Add:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, newIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, oldIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, newIndex, oldIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, newIndex);
                    break;
                default:
                    throw new Exception("bug");
            }

            this.CollectionChanged?.Invoke(this, args);
        }

        private int CalculateOffset(object sender)
        {
            var offset = 0;

            this.itemsLock.EnterReadLock();
            try
            {
                foreach (var coll in this.collections)
                    if (sender == coll)
                        break;
                    else
                        offset += coll.Count();
            }
            finally
            {
                this.itemsLock.ExitReadLock();
            }

            return offset;
        }

        private void NotifyItemsAdded(IList items, int offset)
        {
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, offset));

            this.OnPropertyChanged(nameof(this.Count));
        }

        private void NotifyItemsRemoved(IList removedItems, int offset)
        {
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, offset));

            this.OnPropertyChanged(nameof(this.Count));
        }

        public void NotifyItemChanged(T item)
        {
            var localIndex = this.Select((collectionItem, index) => new { x = collectionItem, index }).FirstOrDefault(x => x.x.Equals(item))?.index ?? -1;
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item, localIndex));
        }

        private void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            if (isDisposed) return;
            this.itemsLock.EnterWriteLock();
            try
            {
                var removedItems = this.ToList();

                this.Count = 0;

                this.collections.OfType<INotifyCollectionChanged>().ForEach(x => x.CollectionChanged -= this.HandleChildCollectionChanged);
                this.collections.Clear();

                removedItems.ForEach(x => x.DisposeIfDisposable());
            }
            finally
            {
                isDisposed = true;
                this.itemsLock.ExitWriteLock();
            }

            this.itemsLock?.Dispose();
        }
    }
}

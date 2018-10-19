using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    /// <remarks>
    /// This implementation supports only set of features used by us.
    /// For instance it will not notify indexer change (http://stackoverflow.com/questions/657675/propertychanged-for-indexer-property) because we do not use such bindings.
    /// The same is applied for other features.
    /// </remarks>
    public class CompositeCollection<T> : IObservableCollection<T>
    {
        private readonly ReaderWriterLockSlim itemsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Object syncRoot;

        private readonly List<IObservableCollection<T>> collections = new List<IObservableCollection<T>>();

        public bool IsReadOnly => true;

        public object this[int index]
        {
            get
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

                throw new IndexOutOfRangeException();
            }

            set => throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            foreach (var coll in this.collections)
                foreach (var item in coll)
                    array.SetValue(item, index++);
        }

        public int Count { get; private set; }
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
            try
            {
                this.itemsLock.EnterWriteLock();

                var removedItems = this.ToList();

                this.collections.OfType<INotifyCollectionChanged>().ForEach(x => x.CollectionChanged -= this.HandleChildCollectionChanged);
                this.collections.Clear();
                this.Count = 0;

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
            try
            {
                this.itemsLock.EnterReadLock();
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

            try
            {
                this.collections.Add(collection);

            }
            finally
            {
                this.itemsLock.ExitWriteLock();
            }

            collection.CollectionChanged += this.HandleChildCollectionChanged;
            var offset = this.Count;

            var addedCollectionCount = collection.Count;
            this.Count += addedCollectionCount;

            this.NotifyItemsAdded(collection, offset);
        }

        private void HandleChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newCount = this.Count + (e.NewItems?.Count ?? 0) - (e.OldItems?.Count ?? 0);

            if (newCount != this.Count)
            {
                this.Count = newCount;
                this.OnPropertyChanged(nameof(this.Count));
            }

            if (this.CollectionChanged == null)
                return;

            var offset = 0;
            foreach (var coll in this.collections)
                if (sender == coll)
                    break;
                else
                    offset += coll.Count();
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

            this.CollectionChanged(this, args);
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
    }
}

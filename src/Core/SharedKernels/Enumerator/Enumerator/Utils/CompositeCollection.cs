using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    // took from https://bitbucket.org/rstarkov/wpfcrutches/src/5d153f4cbce92af5f154d724668ec0e946072119/CompositeCollection.cs?fileviewer=file-view-default

    /// <summary>
    ///     Implements a true observable composite collection. This collection looks like a collection of the underlying
    ///     elements, however the elements are themselves sourced from observable collections. Changes to the
    ///     underlying collections will be immediately reflected in this collection, with all the appropriate change
    ///     notifications.
    /// </summary>
    public class CompositeCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<IList<T>> collections = new List<IList<T>>();

        /// <summary>Returns true, because individual items cannot be added directly to this collection.</summary>
        public bool IsReadOnly => true;

        /// <summary>Gets the number of elements in this collection.</summary>
        public int Count { get; private set; }

        /// <summary>Removes all underlying collections from this collection (without changing them).</summary>
        public void Clear()
        {
            this.collections.OfType<INotifyCollectionChanged>().ForEach(x => x.CollectionChanged-= this.collectionChanged);
            this.collections.Clear();
            this.Count = 0;
            this.propertyChanged("Count");
            this.collectionChanged_Reset();
        }

        /// <summary>
        ///     Gets a value indicating whether the specified item is contained in this collection.
        /// </summary>
        public bool Contains(T item)
        {
            foreach (var coll in this.collections)
                if (coll.Contains(item))
                    return true;
            return false;
        }

        /// <summary>
        ///     Gets the index of the specified item within this collection, or -1 if the item is not present.
        /// </summary>
        public int IndexOf(T item)
        {
            var offset = 0;
            foreach (var coll in this.collections)
            {
                var index = coll.IndexOf(item);
                if (index >= 0)
                    return offset + index;
                offset += coll.Count;
            }
            return -1;
        }

        /// <summary>
        ///     Gets an item at the specified index. Does not support setting an item.
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index >= this.Count)
                    throw new IndexOutOfRangeException();
                foreach (var coll in this.collections)
                {
                    if (index < coll.Count)
                        return coll[index];
                    index -= coll.Count;
                }
                throw new IndexOutOfRangeException();
            }
            set { throw new NotSupportedException(); }
        }

        /// <summary>Enumerates all items in the order in which they appear in the underlying collections.</summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var coll in this.collections)
                foreach (var item in coll)
                    yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>Copies this collection to the specified array.</summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var coll in this.collections)
            {
                coll.CopyTo(array, arrayIndex);
                arrayIndex += coll.Count;
            }
        }

        /// <summary>Throws a "not supported" exception.</summary>
        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws a "not supported" exception.</summary>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws a "not supported" exception.</summary>
        public void Add(T item)
        {
            this.AddCollection(new ObservableCollection<T>(item.ToEnumerable()));
        }

        /// <summary>Throws a "not supported" exception.</summary>
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Triggered whenever the collection is changed (which happens whenever an underlying collection is changed,
        ///     or a new items collection is added via <see cref="AddCollection{TC}" />).
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     Triggered whenever the <see cref="Count" /> property changes (which happens whenever an underlying collection is
        ///     changed,
        ///     or a new items collection is added via <see cref="AddCollection{TC}" />).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Adds an observable collection as an item source for this collection. The items will appear in this collection
        ///     in the same order, after all the items sourced from collections added earlier (if any). Any changes to the
        ///     added collection will be immediately reflected in this collection.
        /// </summary>
        public void AddCollection<TC>(TC collection)
            where TC : IList<T>, INotifyCollectionChanged
        {
            this.collections.Add(collection);
            collection.CollectionChanged += this.collectionChanged;
            var offset = this.Count;
            this.Count += collection.Count;
            this.propertyChanged("Count");
            if (collection.Count > 5)
                this.collectionChanged_Reset();
            else
                for (var i = 0; i < collection.Count; i++)
                    this.collectionChanged_Added(collection[i], offset + i);
        }

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Count = this.Count + (e.NewItems?.Count ?? 0) -
                         (e.OldItems?.Count ?? 0);
            this.propertyChanged("Count");

            if (this.CollectionChanged == null)
                return;

            var offset = 0;
            foreach (var coll in this.collections)
                if (sender == coll)
                    break;
                else
                    offset += coll.Count;
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
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems,
                        oldIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, newIndex,
                        oldIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems,
                        e.OldItems, newIndex);
                    break;
                default:
                    throw new Exception("bug");
            }
            this.CollectionChanged(this, args);
        }

        private void collectionChanged_Added(T item, int index)
        {
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        private void collectionChanged_Moved(T item, int oldIndex, int newIndex)
        {
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        private void collectionChanged_Removed(T item, int index)
        {
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        private void collectionChanged_Reset()
        {
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void propertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
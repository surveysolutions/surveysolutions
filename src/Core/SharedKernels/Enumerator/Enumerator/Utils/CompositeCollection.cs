using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    // took from https://bitbucket.org/rstarkov/wpfcrutches/src/5d153f4cbce92af5f154d724668ec0e946072119/CompositeCollection.cs?fileviewer=file-view-default

    public class CompositeCollection<T> : IObserbableCollection<T>
    {
        private readonly List<IObserbableCollection<T>> collections = new List<IObserbableCollection<T>>();

        public bool IsReadOnly => true;

        public int Count { get; private set; }

        public void Clear()
        {
            this.collections.OfType<INotifyCollectionChanged>().ForEach(x => x.CollectionChanged-= this.collectionChanged);
            this.collections.Clear();
            this.Count = 0;
            this.propertyChanged("Count");
            this.collectionChanged_Reset();
        }

        public bool Contains(T item)
        {
            foreach (var coll in this.collections)
                if (coll.Contains(item))
                    return true;
            return false;
        }

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

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(T item)
        {
            this.AddCollection(new CovariantObservableCollection<T>(item.ToEnumerable()));
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddCollection(IObserbableCollection<T> collection)
        {
            this.collections.Add(collection);
            collection.CollectionChanged += this.collectionChanged;
            var offset = this.Count;
            var addedCollectionCount = collection.Count();
            this.Count += addedCollectionCount;
            this.propertyChanged("Count");
            if (addedCollectionCount > 5)
                this.collectionChanged_Reset();
            else
                for (var i = 0; i < addedCollectionCount; i++)
                    this.collectionChanged_Added(collection.ElementAt(i), offset + i);
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
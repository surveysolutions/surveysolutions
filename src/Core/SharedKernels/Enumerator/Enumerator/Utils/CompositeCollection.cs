using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class CompositeCollection<T> : IObservableCollection<T>
    {
        private const string IndexerName = "Item[]";

        private readonly List<IObservableCollection<T>> collections = new List<IObservableCollection<T>>();

        public bool IsReadOnly => true;

        public int Count { get ; private set; }

        public void Clear()
        {
            this.collections.OfType<INotifyCollectionChanged>().ForEach(x => x.CollectionChanged-= this.collectionChanged);
            this.collections.Clear();
            this.Count = 0;
            this.OnPropertyChanged(nameof(Count));
            this.OnPropertyChanged(IndexerName);
            this.collectionChanged(this, 
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, 
                    this.ToList(),
                    0));
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

        public void Add(T item)
        {
            this.AddCollection(new CovariantObservableCollection<T>(item.ToEnumerable()));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddCollection(IObservableCollection<T> collection)
        {
            this.collections.Add(collection);
            collection.CollectionChanged += this.collectionChanged;
            var offset = this.Count;
            var addedCollectionCount = collection.Count();
            this.Count += addedCollectionCount;
            this.collectionChanged_Added(collection.ToList(), offset);
        }

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Count = this.Count + (e.NewItems?.Count ?? 0) - (e.OldItems?.Count ?? 0);
            this.OnPropertyChanged("Count");

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

        private void collectionChanged_Added(IList<T> items, int offset)
        {
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, offset));
            this.OnPropertyChanged("Count");
        }

        public void NotifyItemChanged(T item)
        {
            var index = this.SkipWhile(x => !x.Equals(item)).Count() - 1;
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item, index));
        }

        private void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
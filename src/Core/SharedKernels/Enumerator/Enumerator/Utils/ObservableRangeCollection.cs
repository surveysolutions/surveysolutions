using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using MvvmCross.Platform.Core;

namespace WB.Core.SharedKernels.Enumerator.Utils
{

    /// <summary> 
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class ObservableRangeCollection<T> : ObservableCollection<T> where T : IDisposable
    {
        private bool _suppressEvents;

        public bool SuppressEvents
        {
            get { return this._suppressEvents; }
            set
            {
                if (this._suppressEvents == value)
                    return;
                this._suppressEvents = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SuppressEvents"));
            }
        }

        protected void InvokeOnMainThread(Action action)
            => MvxSingleton<IMvxMainThreadDispatcher>.Instance?.RequestMainThreadAction(action);

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => this.InvokeOnMainThread(() =>
            {
                if (!this.SuppressEvents)
                    base.OnCollectionChanged(e);
            });

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            => this.InvokeOnMainThread(() => base.OnPropertyChanged(e));

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public ObservableRangeCollection()
            : base()
        {
        }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public ObservableRangeCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            int startIndex = Count;
            var changedItems = collection is List<T> ? (List<T>) collection : new List<T>(collection);
            try
            {
                this.SuppressEvents = true;
                foreach (var i in changedItems)
                {
                    Items.Add(i);
                }
            }
            finally
            {
                this.SuppressEvents = false;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    changedItems, startIndex));
            }
        }

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            var changedItems = collection is List<T> ? (List<T>) collection : new List<T>(collection);
            try
            {
                this.SuppressEvents = true;
                foreach (var i in changedItems)
                {
                    Items.Insert(index++, i);
                }
            }
            finally
            {
                this.SuppressEvents = false;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    changedItems, index));
            }
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
        /// </summary> 
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            var changedItems = collection is List<T> ? (List<T>) collection : new List<T>(collection);
            try
            {
                this.SuppressEvents = true;
                for (int i = 0; i < changedItems.Count; i++)
                {
                    var changedItem = changedItems[i];

                    changedItem.Dispose();
                    if (!Items.Remove(changedItem))
                    {
                        changedItems.RemoveAt(i);
                        //Can't use a foreach because changedItems is intended to be (carefully) modified
                        i--;
                    }
                }
            }
            finally
            {
                this.SuppressEvents = false;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    changedItems, -1));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MvvmCross.Platform.Core;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    /// <summary> 
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class ObservableRangeCollection<T> : ObservableCollection<T> where T: IDisposable
    {
        private bool _suppressEvents;

        public bool SuppressEvents
        {
            get
            {
                return this._suppressEvents;
            }
            set
            {
                if (this._suppressEvents == value)
                    return;
                this._suppressEvents = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SuppressEvents"));
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.SuppressEvents)
                return;

            this.InvokeOnMainThread(() => base.OnCollectionChanged(e));
        }

        protected void InvokeOnMainThread(Action action)
            => MvxSingleton<IMvxMainThreadDispatcher>.Instance?.RequestMainThreadAction(action);

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            => this.InvokeOnMainThread(() => base.OnPropertyChanged(e));

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            this.CheckReentrancy();

            try
            {
                this.SuppressEvents = true;
                foreach (var i in collection) this.Items.Add(i);
            }
            finally
            {
                this.SuppressEvents = false;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary> 
        /// Inserts the collection at specified index of ObservableCollection(Of T). 
        /// </summary> 
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            this.CheckReentrancy();

            try
            {
                this.SuppressEvents = true;
                foreach (var i in collection) this.Items.Insert(index++, i);
            }
            finally
            {
                this.SuppressEvents = false;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). 
        /// </summary> 
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            this.CheckReentrancy();

            try
            {
                this.SuppressEvents = true;
                foreach (var i in collection)
                {
                    i.Dispose();
                    this.Items.Remove(i);
                }
            }
            finally
            {
                this.SuppressEvents = false;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public new void Insert(int index, T item) => this.InvokeOnMainThread(() => base.Insert(index, item));

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public ObservableRangeCollection()
            : base() { }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public ObservableRangeCollection(IEnumerable<T> collection)
            : base(collection) { }
    }
}
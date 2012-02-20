using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    public static class ObservableCollectionExtensions
    {
        public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>>
            GetObservableChanges<T>(this ObservableCollection<T> collection)
        {
            return Observable.FromEventPattern<
                NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                 //   h => new NotifyCollectionChangedEventHandler(h),
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h
                );
        }
       /* public static IObservable<T>
           GetObservablePropertyChanges<T>(this INotifyPropertyChanged collection) where T: class
        {
            return Observable.FromEventPattern<
                PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    //   h => new NotifyCollectionChangedEventHandler(h),
                h => collection.PropertyChanged += h,
                h => collection.PropertyChanged -= h
                ).SelectMany(e => e.EventArgs);
        }*/
        public static IObservable<T> GetObservableAddedValues<T>(
            this ObservableCollection<T> collection)
        {
            return collection.GetObservableChanges()
                .Where(evnt => evnt.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(evnt => evnt.EventArgs.NewItems.Cast<T>());
        }
        public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>> GetObservableRemovedValues<T>(
           this ObservableCollection<T> collection)
        {
            return collection.GetObservableChanges()
                .Where(evnt => evnt.EventArgs.Action == NotifyCollectionChangedAction.Remove);
        }
    }
   /* public class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Unsubscribe(e.OldItems);
            Subscribe(e.NewItems);
            base.OnCollectionChanged(e);
        }

        protected override void ClearItems()
        {
            foreach (T element in this)
                element.PropertyChanged -= ContainedElementChanged;

            base.ClearItems();
        }

        private void Subscribe(IList iList)
        {
            if (iList != null)
            {
                foreach (T element in iList)
                    element.PropertyChanged += ContainedElementChanged;
            }
        }

        private void Unsubscribe(IList iList)
        {
            if (iList != null)
            {
                foreach (T element in iList)
                    element.PropertyChanged -= ContainedElementChanged;
            }
        }

        private void ContainedElementChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }
    }*/
}

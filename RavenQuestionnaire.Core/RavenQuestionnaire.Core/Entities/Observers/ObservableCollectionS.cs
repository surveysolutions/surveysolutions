using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    [Serializable]
    public class ObservableCollectionS<T> : ObservableCollection<T>, INotifyPropertyChanged
    {
        public ObservableCollectionS()
        {
        }

        public ObservableCollectionS(List<T> list) : base(list)
        {
        }

        public ObservableCollectionS(IEnumerable<T> collection) : base(collection)
        {
        }

        [field: NonSerialized]
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        [field: NonSerialized]
        private PropertyChangedEventHandler _propertyChangedEventHandler;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                _propertyChangedEventHandler = Delegate.Combine(_propertyChangedEventHandler, value) as PropertyChangedEventHandler;
            }
            remove
            {
                _propertyChangedEventHandler = Delegate.Remove(_propertyChangedEventHandler, value) as PropertyChangedEventHandler;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
        /*    Unsubscribe(e.OldItems);
            Subscribe(e.NewItems);*/
            NotifyCollectionChangedEventHandler handler = CollectionChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }
   /*     private void Subscribe(IList iList)
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
        }*/
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = _propertyChangedEventHandler;

            if (handler != null)
            {
                handler(this, e);
            }
        }
        public int RemoveAll(Predicate<T> condition)
        {
            List<int> indexesForRemove = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                if (condition(this[i]))
                {
                    indexesForRemove.Add(i);
                }
            }
            foreach (int i in indexesForRemove)
            {
                this.RemoveAt(i);
            }
            return indexesForRemove.Count;
        }
    }
}

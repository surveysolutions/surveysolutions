using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class CovariantObservableCollection<T> : ObservableCollection<T>, IObservableCollection<T>
    {
        private bool collectionChangedEventsSuspended;

        public CovariantObservableCollection()
        {
        }

        public CovariantObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public void SuspendCollectionChanged()
        {
            this.collectionChangedEventsSuspended = true;
        }

        public void ResumeCollectionChanged()
        {
            this.collectionChangedEventsSuspended = false;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this.collectionChangedEventsSuspended)
            {
                base.OnCollectionChanged(e);
            }
        }

        public new void Clear()
        {
            var removedItems = this.ToList();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, 0));

            base.Clear();
        }
    }
}

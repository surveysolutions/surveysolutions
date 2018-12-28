using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class CovariantObservableCollection<T> : MvxObservableCollection<T>, IObservableCollection<T>
    {
        private bool collectionChangedEventsSuspended;

        public CovariantObservableCollection()
        {
        }

        public CovariantObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this.collectionChangedEventsSuspended)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}

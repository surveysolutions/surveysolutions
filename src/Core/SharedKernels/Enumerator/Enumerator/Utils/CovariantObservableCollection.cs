using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class CovariantObservableCollection<T> : ObservableCollection<T>, IObservableCollection<T>
    {
        public CovariantObservableCollection()
        {
        }

        public CovariantObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public new void Clear()
        {
            var removedItems = this.ToList();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, 0));

            base.Clear();
        }
    }
}
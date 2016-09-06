using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class CovariantObservableCollection<T> : ObservableCollection<T>, IObserbableCollection<T>
    {
        public CovariantObservableCollection()
        {
        }

        public CovariantObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }
    }
}
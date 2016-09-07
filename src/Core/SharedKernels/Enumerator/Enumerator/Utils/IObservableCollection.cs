using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public interface IObservableCollection<out T>:  
        IEnumerable<T>, 
        INotifyCollectionChanged, 
        INotifyPropertyChanged
    {
    }
}
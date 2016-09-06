using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public interface IObserbableCollection<out T>:  
        IEnumerable<T>, 
        INotifyCollectionChanged, 
        INotifyPropertyChanged
    {
    }
}
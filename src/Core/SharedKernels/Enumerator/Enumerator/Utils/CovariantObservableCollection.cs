using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class CovariantObservableCollection<T> : MvxObservableCollection<T>, IObservableCollection<T>
    {
        public CovariantObservableCollection()
        {
        }

        public CovariantObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        protected override Task InvokeOnMainThread(Action action)
        {
            return base.InvokeOnMainThread(action);
        }
    }
}

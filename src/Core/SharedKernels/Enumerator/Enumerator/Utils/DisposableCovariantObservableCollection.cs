using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class DisposableCovariantObservableCollection<T> : CovariantObservableCollection<T>, IDisposable
    {
        private readonly IDisposable disposable;

        public DisposableCovariantObservableCollection(IDisposable disposable)
        {
            this.disposable = disposable;
        }

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public interface ISynchronizationCompleteSource
    {
        IObservable<bool> SynchronizationEvents { get; }
        void NotifyOnCompletedSynchronization(bool isSuccess);
    }

    class SynchronizationCompleteSource : ISynchronizationCompleteSource
    {
        private readonly Subject<bool> synchronizationEvent = new Subject<bool>();

        public SynchronizationCompleteSource()
        {
            SynchronizationEvents = synchronizationEvent.AsObservable();
        }

        public IObservable<bool> SynchronizationEvents { get; }

        public void NotifyOnCompletedSynchronization(bool isSuccess)
        {
            synchronizationEvent.OnNext(isSuccess);
        }
    }
}

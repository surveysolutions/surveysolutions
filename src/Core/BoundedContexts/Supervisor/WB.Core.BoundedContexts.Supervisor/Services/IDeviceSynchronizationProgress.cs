using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IDeviceSynchronizationProgress
    {
        IObservable<DeviceSyncStats> SyncStats { get; }
        void Publish(DeviceSyncStats stats);
    }

    public class DeviceSynchronizationProgress : IDeviceSynchronizationProgress
    {
        private readonly Subject<DeviceSyncStats> synchronizationEvent = new Subject<DeviceSyncStats>();

        public DeviceSynchronizationProgress()
        {
            this.SyncStats = synchronizationEvent.AsObservable();
        }

        public void Publish(DeviceSyncStats stats)
        {
            this.synchronizationEvent.OnNext(stats);

        }

        public IObservable<DeviceSyncStats> SyncStats { get; }
    }

    public class DeviceSyncStats
    {
        public string InterviewerLogin { get; set; }

        public SyncProgressInfo ProgressInfo { get; set; }
    }
}

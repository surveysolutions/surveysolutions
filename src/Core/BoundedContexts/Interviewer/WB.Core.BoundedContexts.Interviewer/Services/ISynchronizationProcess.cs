using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISynchronizationProcess
    {
        Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken);
    }

    public class SyncProgressInfo
    {
        public SyncProgressInfo()
        {
            this.Statistics = new SychronizationStatistics();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public SynchronizationStatus Status { get; set; }
        public SychronizationStatistics Statistics { get; set; }
        public bool UserIsLinkedToAnotherDevice { get; set; }

        public bool IsRunning
        {
            get
            {
                return this.Status == SynchronizationStatus.Download || this.Status == SynchronizationStatus.Started ||
                       this.Status == SynchronizationStatus.Upload;
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface ISynchronizationProcess
    {
        Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken);
    }

    public class SyncProgressInfo
    {
        public SyncProgressInfo()
        {
            this.Statistics = new SynchronizationStatistics();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public SynchronizationStatus Status { get; set; }
        public SynchronizationStatistics Statistics { get; set; }
        public bool UserIsLinkedToAnotherDevice { get; set; }

        public bool HasErrors => this.Statistics.FailedToUploadInterviwesCount != 0 || 
                                 this.Statistics.FailedToCreateInterviewsCount != 0;

        public bool IsRunning => this.Status == SynchronizationStatus.Download || 
                                 this.Status == SynchronizationStatus.Started ||
                                 this.Status == SynchronizationStatus.Upload;
    }
}

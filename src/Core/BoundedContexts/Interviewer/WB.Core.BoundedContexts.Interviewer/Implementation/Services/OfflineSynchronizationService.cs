using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class OfflineSynchronizationService : IOfflineSynchronizationService
    {
        private readonly IInterviewsSynchronizer interviewsSynchronizer;

        public OfflineSynchronizationService(IInterviewsSynchronizer interviewsSynchronizer)
        {
            this.interviewsSynchronizer = interviewsSynchronizer;
        }

        public async Task SynchronizeAsync(string endpoint, IProgress<OfflineSynchronizationProgress> progress)
        {
            await this.interviewsSynchronizer.UploadPendingInterviews(endpoint, new Progress<UploadProgress>());

        }
    }
}

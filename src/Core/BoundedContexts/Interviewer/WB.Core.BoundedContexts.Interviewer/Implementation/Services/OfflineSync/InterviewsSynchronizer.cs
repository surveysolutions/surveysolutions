using System;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync
{
    public class InterviewsSynchronizer : IInterviewsSynchronizer
    {
        private readonly IPlainStorage<InterviewView> interviewStorage;
        private readonly IInterviewerInterviewAccessor interviewAccessor;
        private readonly IOfflineSyncClient syncClient;

        public InterviewsSynchronizer(IPlainStorage<InterviewView> interviewStorage,
            IInterviewerInterviewAccessor interviewAccessor,
            IOfflineSyncClient syncClient)
        {
            this.interviewStorage = interviewStorage;
            this.interviewAccessor = interviewAccessor;
            this.syncClient = syncClient;
        }

        public async Task UploadPendingInterviews(string endpoint, IProgress<UploadProgress> progress)
        {
            var interviews = this.interviewStorage.Where(interview => interview.Status == InterviewStatus.Completed).ToArray();
            for (int i = 0; i < interviews.Length; i++)
            {
                var postedInterviewId = interviews[i].InterviewId;
                var interviewPackage = this.interviewAccessor.GetPendingInteviewEvents(postedInterviewId);
                await this.syncClient.PostInterviewAsync(endpoint, new PostInterviewRequest(postedInterviewId, interviewPackage)).ConfigureAwait(false);
                this.interviewAccessor.RemoveInterview(postedInterviewId);

                progress.Report(new UploadProgress(i + 1, interviews.Length));
            }

        }
    }
}

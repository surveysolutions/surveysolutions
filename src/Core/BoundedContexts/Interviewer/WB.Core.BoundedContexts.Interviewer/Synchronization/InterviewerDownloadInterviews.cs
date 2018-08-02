using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class InterviewerDownloadInterviews : DownloadInterviews
    {
        public InterviewerDownloadInterviews(ISynchronizationService synchronizationService,
            IQuestionnaireDownloader questionnaireDownloader, 
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository, 
            IPlainStorage<InterviewView> interviewViewRepository, 
            ILiteEventBus eventBus, 
            IEnumeratorEventStorage eventStore, 
            ILogger logger, 
            IInterviewsRemover interviewsRemover, int sortOrder) : base(synchronizationService, questionnaireDownloader, interviewSequenceViewRepository, interviewViewRepository, eventBus, eventStore, logger, interviewsRemover, sortOrder)
        {
        }

        protected override async Task<List<Guid>> FindObsoleteInterviewsAsync(IEnumerable<InterviewView> localInterviews, IEnumerable<InterviewApiView> remoteInterviews, IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_CheckForObsolete_Interviews
            });

            var lastKnownEventsWithInterviewIds = localInterviews.Select(x => new ObsoletePackageCheck
                {
                    InterviewId = x.InterviewId,
                    SequenceOfLastReceivedEvent = this.EventStore.GetLastEventKnownToHq(x.InterviewId)
                }).Where(x => x.SequenceOfLastReceivedEvent > 0)
                .ToList();
            var obsoleteInterviews = await this.SynchronizationService.CheckObsoleteInterviewsAsync(lastKnownEventsWithInterviewIds, cancellationToken);

            obsoleteInterviews = obsoleteInterviews.Concat(
                remoteInterviews.Where(x => localInterviews.Any(local => local.InterviewId == x.Id && local.ResponsibleId != x.ResponsibleId))
                    .Select(x => x.Id)
            ).ToList();

            return obsoleteInterviews;
        }
    }
}

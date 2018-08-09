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

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SupervisorDownloadInterviews : DownloadInterviews
    {
        public SupervisorDownloadInterviews(ISynchronizationService synchronizationService, IQuestionnaireDownloader questionnaireDownloader, IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository, IPlainStorage<InterviewView> interviewViewRepository, ILiteEventBus eventBus, IEnumeratorEventStorage eventStore, ILogger logger, IInterviewsRemover interviewsRemover, int sortOrder) : base(synchronizationService, questionnaireDownloader, interviewSequenceViewRepository, interviewViewRepository, eventBus, eventStore, logger, interviewsRemover, sortOrder)
        {
        }

        protected override Task<List<Guid>> FindObsoleteInterviewsAsync(IEnumerable<InterviewView> localInterviews, IEnumerable<InterviewApiView> remoteInterviews, IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }
    }
}

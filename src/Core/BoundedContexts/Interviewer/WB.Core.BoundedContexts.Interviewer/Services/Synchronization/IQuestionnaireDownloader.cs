using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public interface IQuestionnaireDownloader
    {
        Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity,
            CancellationToken cancellationToken, SynchronizationStatistics statistics);
    }
}
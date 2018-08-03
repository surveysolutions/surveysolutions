using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface IQuestionnaireDownloader
    {
        Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity,
            SynchronizationStatistics statistics, IProgress<TransferProgress> transferProgress,
            CancellationToken cancellationToken);
    }
}

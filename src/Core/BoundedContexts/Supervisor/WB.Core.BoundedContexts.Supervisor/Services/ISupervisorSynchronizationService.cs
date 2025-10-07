using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ISupervisorSynchronizationService : ISynchronizationService
    {
        Task<SupervisorApiView> GetSupervisorAsync(RestCredentials credentials = null, CancellationToken token = default);
        Task<List<InterviewerFullApiView>> GetInterviewersAsync(CancellationToken cancellationToken = default);
        Task UploadBrokenInterviewPackageAsync(BrokenInterviewPackageApiView brokenInterviewPackage, CancellationToken cancellationToken = default);
        Task UploadBrokenImagePackageAsync(BrokenImagePackageApiView brokenImagePackage, CancellationToken token = default);
        Task UploadBrokenAudioPackageAsync(BrokenAudioPackageApiView brokenAudioPackage, CancellationToken token = default);
        Task UploadBrokenAudioAuditPackageAsync(BrokenAudioAuditPackageApiView brokenAudioAuditPackage, CancellationToken token = default);
        Task UploadInterviewerExceptionsAsync(List<UnexpectedExceptionFromInterviewerView> exceptions, CancellationToken cancellationToken = default);
        Task UploadTabletInfoAsync(DeviceInfoApiView deviceInfoApiView, CancellationToken cancellationToken = default);
        Task UploadInterviewerSyncStatistic(InterviewerSyncStatisticsApiView statisticToSend, CancellationToken cancellationToken = default);
        Task<List<string>> GetListOfDeletedQuestionnairesIds(CancellationToken cancellationToken = default);
        Task<byte[]> GetInterviewerApplicationAsync(byte[] existingFileHash, IProgress<TransferProgress> transferProgress = null, CancellationToken token = default);
        Task<byte[]> GetInterviewerApplicationWithMapsAsync(byte[] existingFileHash, IProgress<TransferProgress> transferProgress = null, CancellationToken token = default);
    }
}

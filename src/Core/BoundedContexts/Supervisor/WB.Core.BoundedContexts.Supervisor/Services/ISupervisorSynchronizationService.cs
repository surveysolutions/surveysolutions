using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ISupervisorSynchronizationService : IOnlineSynchronizationService
    {
        Task<SupervisorApiView> GetSupervisorAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task<List<InterviewerFullApiView>> GetInterviewersAsync(CancellationToken cancellationToken);
        Task UploadBrokenInterviewPackageAsync(BrokenInterviewPackageApiView brokenInterviewPackage, CancellationToken cancellationToken);
        Task UploadInterviewerExceptionsAsync(List<UnexpectedExceptionFromInterviewerView> exceptions, CancellationToken cancellationToken);
        Task UploadTabletInfoAsync(DeviceInfoApiView deviceInfoApiView, CancellationToken cancellationToken);
        Task UploadInterviewerSyncStatistic(InterviewerSyncStatisticsApiView statisticToSend,
            CancellationToken cancellationToken);

        Task<List<string>> GetListOfDeletedQuestionnairesIds(CancellationToken cancellationToken);
        Task<InterviewerApplicationPatchApiView[]> GetListOfInterviewerAppPatchesAsync(CancellationToken cancellationToken);

        Task<byte[]> GetInterviewerApplicationPatchByNameAsync(string patchName, CancellationToken token,
            IProgress<TransferProgress> transferProgress);
        Task<int?> GetLatestInterviewerAppVersionAsync(CancellationToken token);
    }
}

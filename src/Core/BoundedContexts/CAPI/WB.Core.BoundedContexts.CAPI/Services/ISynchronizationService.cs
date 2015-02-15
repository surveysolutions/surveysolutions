using System;
using System.Threading.Tasks;

using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ISynchronizationService
    {
        Task<HandshakePackage> HandshakeAsync(SyncCredentials credentials, bool shouldThisDeviceBeLinkedToUser = false);

        Task<UserSyncPackageDto> RequestUserPackageAsync(SyncCredentials credentials, string chunkId);

        Task<QuestionnaireSyncPackageDto> RequestQuestionnairePackageAsync(SyncCredentials credentials, string chunkId);

        Task<InterviewSyncPackageDto> RequestInterviewPackageAsync(SyncCredentials credentials, string chunkId);

        Task PushChunkAsync(SyncCredentials credentials, string chunkAsString);

        Task PushBinaryAsync(SyncCredentials credentials, Guid interviewId, string fileName, byte[] fileData);

        Task<bool> NewVersionAvailableAsync();

        Task<bool> CheckExpectedDeviceAsync(SyncCredentials credentials);

        Task<SyncItemsMetaContainer> GetPackageIdsToDownloadAsync(SyncCredentials credentials, string type, string lastSyncedPackageId);
    }
}

using System;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;

namespace WB.Core.Synchronization
{
    public interface ISyncManager
    {
        HandshakePackage InitSync(ClientIdentifier identifier);

        void SendSyncItem(Guid interviewId, string package);

        SyncItemsMetaContainer GetQuestionnairePackageIdsWithOrder(Guid userId, Guid clientRegistrationId, string lastSyncedPackageId);

        SyncItemsMetaContainer GetUserPackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId);

        SyncItemsMetaContainer GetInterviewPackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId);

        UserSyncPackageDto ReceiveUserSyncPackage(Guid deviceId, string packageId, Guid userId);

        QuestionnaireSyncPackageDto ReceiveQuestionnaireSyncPackage(Guid deviceId, string packageId, Guid userId);

        InterviewSyncPackageDto ReceiveInterviewSyncPackage(Guid deviceId, string packageId, Guid userId);

        void LinkUserToDevice(Guid interviewerId, string androidId, string appVersion, string oldDeviceId);
    }
}

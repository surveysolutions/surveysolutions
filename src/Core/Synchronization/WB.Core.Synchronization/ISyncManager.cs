using System;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;

namespace WB.Core.Synchronization
{
    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);

        void SendSyncItem(string package);

        SyncItemsMetaContainer GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationId, string lastSyncedUserPackageId, string lastSyncedQuestionnairePackageId, string lastSyncedInterviewPackageId);

        UserSyncPackageDto ReceiveUserSyncPackage(Guid clientRegistrationId, string packageId);

        QuestionnaireSyncPackageDto ReceiveQuestionnaireSyncPackage(Guid clientRegistrationId, string packageId);

        InterviewSyncPackageDto ReceiveInterviewSyncPackage(Guid clientRegistrationId, string packageId);

        string GetPackageIdByTimestamp(Guid userId, DateTime timestamp);

        void LinkUserToDevice(Guid interviewerId, string deviceId);
    }
}

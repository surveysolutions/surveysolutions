using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class SyncItemsMetaContainerRequest
    {
        public Guid ClientRegistrationId { get; set; }
        public string LastSyncedUserPackageId { get; set; }
        public string LastSyncedQuestionnairePackageId { get; set; }
        public string LastSyncedInterviewPackageId { get; set; }
    }
}

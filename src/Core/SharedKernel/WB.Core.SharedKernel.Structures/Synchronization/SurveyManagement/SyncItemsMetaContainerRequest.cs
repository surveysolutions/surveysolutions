using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class SyncItemsMetaContainerRequest
    {
        public Guid ClientRegistrationId { get; set; }
        public string LastSyncedPackageId { get; set; }
    }
}

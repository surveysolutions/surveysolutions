using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class SyncPackageRequest
    {
        public string PackageId { get; set; }
        public string PreviousSuccessfullyHandledPackageId { get; set; }
        public Guid ClientRegistrationId { get; set; }
    }
}

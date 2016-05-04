using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class MarkPackageAsSuccessfullyHandledRequest
    {
        public string PackageId { get; set; }
        public Guid ClientRegistrationId { get; set; }
    }
}
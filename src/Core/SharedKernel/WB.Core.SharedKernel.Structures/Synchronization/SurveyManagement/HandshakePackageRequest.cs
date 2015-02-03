using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class HandshakePackageRequest
    {
        public Guid ClientId { get; set; }
        public string AndroidId { get; set; }
        public Guid? ClientRegistrationId { get; set; }
        public int Version { get; set; }
        public bool ShouldDeviceBeLinkedToUser { get; set; }
    }
}

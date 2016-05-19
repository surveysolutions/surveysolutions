using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    [Obsolete]
    public class HandshakePackageRequest
    {
        public string AndroidId { get; set; }
        public int Version { get; set; }
        public bool ShouldDeviceBeLinkedToUser { get; set; }
    }
}

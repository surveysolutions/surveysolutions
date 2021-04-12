using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class WorkspaceUserProfile
    {
        public int Id { get; protected set; }
        public Guid? SupervisorId { get; protected set; }
        public string DeviceId { get; protected set; }
        public DateTime? DeviceRegistrationDate { get; protected set; }
        public string DeviceAppVersion { get; protected set; }
        public int? DeviceAppBuildVersion { get; protected set; }
        public long? StorageFreeInBytes { get; protected set; }
    }
}
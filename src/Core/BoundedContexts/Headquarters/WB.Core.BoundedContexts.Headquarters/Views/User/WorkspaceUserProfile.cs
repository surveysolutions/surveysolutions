using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class WorkspaceUserProfile
    {
        public virtual int Id { get; protected set; }
        public virtual Guid? SupervisorId { get; protected set; }
        public virtual string DeviceId { get; protected set; }
        public virtual DateTime? DeviceRegistrationDate { get; protected set; }
        public virtual string DeviceAppVersion { get; protected set; }
        public virtual int? DeviceAppBuildVersion { get; protected set; }
        public virtual long? StorageFreeInBytes { get; protected set; }
    }
}
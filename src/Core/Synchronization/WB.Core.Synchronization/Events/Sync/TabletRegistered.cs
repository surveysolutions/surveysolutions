using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Synchronization.Events.Sync
{   
    public class HandshakeRequested : ILiteEvent
    {
        public HandshakeRequested(Guid userId, string appVersion)
        {
            this.UserId = userId;
            this.AppVersion = appVersion;
        }

        public Guid UserId { get; set; }

        public string AppVersion { get; set; }
    }

    public class PackageRequested : ILiteEvent
    {
        public PackageRequested(Guid userId, string packageType, string packageId)
        {
            this.PackageId = packageId;
            this.UserId = userId;
            this.PackageType = packageType;
        }

        public Guid UserId { get; set; }

        public string PackageType { get; set; }

        public string PackageId { get; set; }
    }

    public class PackageIdsRequested : ILiteEvent
    {
        public PackageIdsRequested(Guid userId, string packageType, string lastSyncedPackageId, string[] updateFromLastPakage)
        {
            this.LastSyncedPackageId = lastSyncedPackageId;
            this.UserId = userId;
            this.PackageType = packageType;
            this.UpdateFromLastPakage = updateFromLastPakage;
        }

        public Guid UserId { get; set; }

        public string PackageType { get; set; }

        public string LastSyncedPackageId { get; set; }

        public string[] UpdateFromLastPakage { get; set; }
    }

    public class UserLinkedFromDevice : ILiteEvent
    {
        public UserLinkedFromDevice(Guid userId)
        {
            this.UserId = userId;
        }

        public Guid UserId { get; set; }
    }

    public class UserLinkingRequested : ILiteEvent
    {
        public UserLinkingRequested(Guid userId)
        {
            this.UserId = userId;
        }

        public Guid UserId { get; set; }
    }

    public class TabletRegistered : ILiteEvent
    {
        public TabletRegistered(string androidId, string appVersion)
        {
            this.AndroidId = androidId;
            this.AppVersion = appVersion;
        }

        public string AndroidId { get; set; }

        public string AppVersion { get; set; }
    }
}

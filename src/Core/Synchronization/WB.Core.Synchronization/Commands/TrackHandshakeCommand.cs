using System;

using Ncqrs.Commanding;

namespace WB.Core.Synchronization.Commands
{
    public class UnlinkUserFromDeviceCommand  : CommandBase
    {
        public Guid DeviceId { get; set; }

        public Guid UserId { get; set; }

        public UnlinkUserFromDeviceCommand(Guid deviceId, Guid userId)
        {
            this.DeviceId = deviceId;
            this.UserId = userId;
        }
    }

    public class TrackUserLinkingRequestCommand  : CommandBase
    {
        public Guid DeviceId { get; set; }

        public Guid UserId { get; set; }

        public TrackUserLinkingRequestCommand(Guid deviceId, Guid userId)
        {
            this.DeviceId = deviceId;
            this.UserId = userId;
        }
    }

    public class TrackPackageRequestCommand  : CommandBase
    {
        public Guid DeviceId { get; set; }

        public Guid UserId { get; set; }

        public string PackageId { get; set; }

        public string PackageType { get; set; }

        public TrackPackageRequestCommand(Guid deviceId, Guid userId, string packageType, string packageId)
        {
            this.DeviceId = deviceId;
            this.UserId = userId;
            this.PackageType = packageType;
            this.PackageId = packageId;
        }
    }

    public class TrackArIdsRequestCommand  : CommandBase
    {
        public Guid DeviceId { get; set; }

        public Guid UserId { get; set; }

        public string LastSyncedPackageId { get; set; }

        public string[] UpdateFromLastPakage { get; set; }

        public string PackageType { get; set; }

        public TrackArIdsRequestCommand(Guid deviceId, Guid userId, string packageType, string lastSyncedPackageId, string[] updateFromLastPakage)
        {
            this.DeviceId = deviceId;
            this.UserId = userId;
            this.PackageType = packageType;
            this.LastSyncedPackageId = lastSyncedPackageId;
            this.UpdateFromLastPakage = updateFromLastPakage;
        }
    }

    public class TrackHandshakeCommand  : CommandBase
    {
        public Guid DeviceId { get; set; }

        public Guid UserId { get; set; }

        public string AppVersion { get; set; }

        public TrackHandshakeCommand(Guid deviceId, Guid userId, string appVersion)
        {
            this.DeviceId = deviceId;
            this.UserId = userId;
            this.AppVersion = appVersion;
        }
    }
}
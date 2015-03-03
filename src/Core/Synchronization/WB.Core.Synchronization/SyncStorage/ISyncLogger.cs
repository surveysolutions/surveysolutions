using System;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface ISyncLogger
    {
        void TrackDeviceRegistration(Guid deviceId, Guid userId, string appVersion, string androidId);

        void TrackUserLinkingRequest(Guid deviceId, Guid userId);

        void TrackArIdsRequest(Guid deviceId, Guid userId, string packageType, string lastSyncedPackageId, string[] packagesToDownload);

        void UnlinkUserFromDevice(Guid oldDeviceId, Guid userId);

        void TrackPackageRequest(Guid deviceId, Guid userId, string packageType, string packageId);

        void TraceHandshake(Guid deviceId, Guid userId, string appVersion);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.SyncLogger
{
    internal class SyncLogger : ISyncLogger
    {
        readonly IPlainStorageAccessor<TabletSyncLog> tabletLogWriter;
        const int LastSyncLimit = 15;

        public SyncLogger(IPlainStorageAccessor<TabletSyncLog> tabletLogWriter)
        {
            this.tabletLogWriter = tabletLogWriter;
        }

        public void TrackDeviceRegistration(Guid deviceId, Guid userId, string appVersion, string androidId)
        {
            this.UpdateState(deviceId, currentState =>
            {
                var updatedState = new TabletSyncLog
                {
                    DeviceId = deviceId.FormatGuid(),
                    AndroidId = androidId,
                    RegistrationDate = DateTime.Now
                };

                updatedState.RegisteredUsersOnDevice.Add(userId);
                return updatedState;
            });
        }

        public void TrackUserLinkingRequest(Guid deviceId, Guid userId)
        {
            this.UpdateState(deviceId, currentState =>
            {
                if (!currentState.RegisteredUsersOnDevice.Contains(userId))
                {
                    currentState.RegisteredUsersOnDevice.Add(userId);
                }
                return currentState;
            });
        }

        public void UnlinkUserFromDevice(Guid deviceId, Guid userId)
        {
            this.UpdateState(deviceId, currentState =>
            {
                var syncLogByUser = currentState.UserSyncLog.FirstOrDefault(x => x.UserId == userId.FormatGuid());
                if (syncLogByUser != null)
                {
                    currentState.UserSyncLog.Remove(syncLogByUser);
                }
                return currentState;
            });
        }

        public void TrackArIdsRequest(Guid deviceId, Guid userId, string packageType,
            string[] updateFromLastPakage)
        {
            this.UpdateState(deviceId, currentState =>
            {
                if (!currentState.RegisteredUsersOnDevice.Contains(userId))
                    return currentState;

                var lastUserSyncLog = currentState.UserSyncLog.LastOrDefault(x => x.UserId == userId.FormatGuid());
                if (lastUserSyncLog == null)
                    return currentState;

                foreach (var packageId in updateFromLastPakage)
                {
                    lastUserSyncLog.PackagesTrackingInfo.Add(new SyncPackageTrackingInfo()
                    {
                        PackageId = packageId,
                        PackageType = packageType
                    });
                }

                return currentState;
            });
        }

        public void TrackPackageRequest(Guid deviceId, Guid userId, string packageType, string packageId)
        {
            this.UpdateState(deviceId, currentState =>
            {
                if (!currentState.RegisteredUsersOnDevice.Contains(userId))
                    return currentState;

                var lastUserSyncLog = currentState.UserSyncLog.LastOrDefault(x => x.UserId == userId.FormatGuid());
                if (lastUserSyncLog == null)
                    return currentState;

                var package = lastUserSyncLog.PackagesTrackingInfo.FirstOrDefault(x => x.PackageId == packageId);

                if (package != null)
                    package.PackageSyncTime = DateTime.Now;

                return currentState;
            });
        }

        public void TraceHandshake(Guid deviceId, Guid userId, string appVersion)
        {
            this.UpdateState(deviceId, currentState =>
            {
                if (!currentState.RegisteredUsersOnDevice.Contains(userId))
                    currentState.RegisteredUsersOnDevice.Add(userId);

                currentState.UserSyncLog.Add(new TabletSyncLogByUser()
                {
                    AppVersion = appVersion,
                    HandshakeTime = DateTime.Now,
                    UserId = userId.FormatGuid()
                });

                for (int i = 0; i < currentState.UserSyncLog.Count - LastSyncLimit; i++)
                {
                    currentState.UserSyncLog.RemoveAt(i);
                }

                return currentState;
            });
        }

        public void MarkPackageAsSuccessfullyHandled(Guid deviceId, Guid userId, string successfullyHandledPackageId)
        {
            this.UpdateState(deviceId, currentState =>
            {
                if (!currentState.RegisteredUsersOnDevice.Contains(userId))
                    return currentState;

                var lastUserSyncLog = currentState.UserSyncLog.LastOrDefault(x => x.UserId == userId.FormatGuid());
                if (lastUserSyncLog == null)
                    return currentState;

                var package = lastUserSyncLog.PackagesTrackingInfo.FirstOrDefault(x => x.PackageId == successfullyHandledPackageId);

                if (package != null)
                    package.ReceivedByClient = true;

                return currentState;
            });
        }

        void UpdateState(Guid deviceId, Func<TabletSyncLog, TabletSyncLog> updateState)
        {
            TabletSyncLog currentState = tabletLogWriter.GetById(deviceId.FormatGuid()) ?? new TabletSyncLog
            {
                RegistrationDate = DateTime.Now,
                DeviceId = deviceId.FormatGuid()
            };
            var updatedState = updateState(currentState);
            updatedState.LastUpdateDate = DateTime.Now;
            tabletLogWriter.Store(updatedState, deviceId.FormatGuid());
        }
    }
}

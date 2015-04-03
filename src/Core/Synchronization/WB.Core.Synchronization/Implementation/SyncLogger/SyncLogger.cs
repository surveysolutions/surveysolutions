using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.SyncLogger
{
    internal class SyncLogger : ISyncLogger
    {
        private readonly IReadSideKeyValueStorage<TabletSyncLogByUsers> tabletLogWriter;
        private const int LastSyncLimit = 15;

        public SyncLogger(IReadSideKeyValueStorage<TabletSyncLogByUsers> tabletLogWriter)
        {
            this.tabletLogWriter = tabletLogWriter;
        }

        public void TrackDeviceRegistration(Guid deviceId, Guid userId, string appVersion, string androidId)
        {
            this.UpdateState(deviceId, currentState =>
               {
                   var updatedState = new TabletSyncLogByUsers
                   {
                       AndroidId = androidId,
                       RegistrationDate = DateTime.Now
                   };

                   updatedState.Users.Add(userId);
                   return updatedState;
               });
        }

        public void TrackUserLinkingRequest(Guid deviceId, Guid userId)
        {
            this.UpdateState(deviceId, currentState =>
              {
                  if (currentState.Users.All(x => x != userId))
                  {
                      currentState.Users.Add(userId);
                  }
                  return currentState;
              });
        }

        public void UnlinkUserFromDevice(Guid deviceId, Guid userId)
        {
            this.UpdateState(deviceId, currentState =>
               {
                   currentState.Users.Remove(userId);
                   return currentState;
               });
        }

        public void TrackArIdsRequest(Guid deviceId, Guid userId, string packageType, string lastSyncedPackageId, string[] updateFromLastPakage)
        {
            this.UpdateState(deviceId, currentState =>
            {
                if (!currentState.SyncLog.ContainsKey(userId))
                    return currentState;

                TabletSyncLog lastUserSyncLog = currentState.SyncLog[userId].Last();
                if (lastUserSyncLog == null)
                    return currentState;

                PackagesTrackingInfo lastPackageInfo = lastUserSyncLog.PackagesTrackingInfo[packageType];

                lastPackageInfo.LastPackageId = lastSyncedPackageId;
                foreach (var packageId in updateFromLastPakage)
                {
                    lastPackageInfo.PackagesRequestInfo.Add(packageId, null);
                }

                return currentState;
            });
        }

        public void TrackPackageRequest(Guid deviceId, Guid userId, string packageType, string packageId)
        {
            this.UpdateState(deviceId, currentState =>
               {
                   if (!currentState.SyncLog.ContainsKey(userId))
                       return currentState;

                   TabletSyncLog lastUserSyncLog = currentState.SyncLog[userId].Last();
                   if (lastUserSyncLog == null)
                       return currentState;

                   PackagesTrackingInfo lastPackageInfo = lastUserSyncLog.PackagesTrackingInfo[packageType];

                   if (lastPackageInfo.PackagesRequestInfo.ContainsKey(packageId))
                       lastPackageInfo.PackagesRequestInfo[packageId] = DateTime.Now;

                   return currentState;
               });
        }

        public void TraceHandshake(Guid deviceId, Guid userId, string appVersion)
        {
            this.UpdateState(deviceId, currentState =>
               {
                   if (!currentState.SyncLog.ContainsKey(userId))
                   {
                       currentState.SyncLog.Add(userId, new List<TabletSyncLog>());
                   }

                   var syncInfoCount = currentState.SyncLog[userId].Count;

                   var syncTail = syncInfoCount < LastSyncLimit 
                       ? currentState.SyncLog[userId]
                       : currentState.SyncLog[userId].Skip(syncInfoCount - LastSyncLimit + 1).Take(LastSyncLimit - 1).ToList();

                   syncTail.Add(new TabletSyncLog { AppVersion = appVersion, HandshakeTime = DateTime.Now });
                   
                   currentState.SyncLog[userId] = syncTail;

                   return currentState;
               });
        }

        private void UpdateState(Guid deviceId, Func<TabletSyncLogByUsers, TabletSyncLogByUsers> updateState)
        {
            TabletSyncLogByUsers currentState = tabletLogWriter.GetById(deviceId) ?? new TabletSyncLogByUsers
                                                                                     {
                                                                                         RegistrationDate = DateTime.Now
                                                                                     };
            var updatedState = updateState(currentState);
            updatedState.LastUpdateDate = DateTime.Now;
            tabletLogWriter.Store(updatedState, deviceId);
        }
    }
}

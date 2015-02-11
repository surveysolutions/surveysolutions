using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Core.Synchronization.EventHandler
{
    public class TabletDenormalizer : AbstractFunctionalEventHandler<TabletDocument, IReadSideRepositoryWriter<TabletDocument>>,
         IUpdateHandler<TabletDocument, HandshakeRequested>,
         IUpdateHandler<TabletDocument, PackageIdsRequested>,
         IUpdateHandler<TabletDocument, UserLinkingRequested>,
         IUpdateHandler<TabletDocument, UnlinkUserFromDeviceCommand>,
         IUpdateHandler<TabletDocument, TabletRegistered>,
        IUpdateHandler<TabletDocument, PackageRequested>
    {

        public TabletDenormalizer(IReadSideRepositoryWriter<TabletDocument> tabletDocumentsStroraWriter) : base(tabletDocumentsStroraWriter)
        {
        }

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<TabletRegistered> evnt)
        {
            return new TabletDocument
                   {
                       AndroidId = evnt.Payload.AndroidId,
                       DeviceId = evnt.EventSourceId,
                       RegistrationDate = evnt.EventTimeStamp,
                       LastUpdateDate = evnt.EventTimeStamp,
                       Users = new List<Guid>()
                   };
        }

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<HandshakeRequested> evnt)
        {
            currentState.LastUpdateDate = evnt.EventTimeStamp;
            Guid userId = evnt.Payload.UserId;

            if (!currentState.SyncLog.ContainsKey(userId))
            {
                currentState.SyncLog.Add(userId, new List<TabletSyncLog>());
            }
            currentState.SyncLog[userId].Add(new TabletSyncLog { AppVersion = evnt.Payload.AppVersion, HandshakeTime = evnt.EventTimeStamp });
            
            return currentState;
        }

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<PackageRequested > evnt)
        {
            currentState.LastUpdateDate = evnt.EventTimeStamp;
            Guid userId = evnt.Payload.UserId;
            if (!currentState.SyncLog.ContainsKey(userId)) 
                return currentState;

            TabletSyncLog lastUserSyncLog = currentState.SyncLog[userId].Last();
            if (lastUserSyncLog == null) 
                return currentState;

            PackagesTrackingInfo lastPackageInfo = lastUserSyncLog.PackagesTrackingInfo[evnt.Payload.PackageType];

            if (lastPackageInfo.PackagesRequestInfo.ContainsKey(evnt.Payload.PackageId))
                lastPackageInfo.PackagesRequestInfo.Add(evnt.Payload.PackageId, evnt.EventTimeStamp);

            return currentState;
        }

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<PackageIdsRequested> evnt)
        {
            currentState.LastUpdateDate = evnt.EventTimeStamp;
            Guid userId = evnt.Payload.UserId;
            if (!currentState.SyncLog.ContainsKey(userId))
                return currentState;

            TabletSyncLog lastUserSyncLog = currentState.SyncLog[userId].Last();
            if (lastUserSyncLog == null)
                return currentState;

            PackagesTrackingInfo lastPackageInfo = lastUserSyncLog.PackagesTrackingInfo[evnt.Payload.PackageType];

            lastPackageInfo.LastPackageId = evnt.Payload.LastSyncedPackageId;
            foreach (var packageId in evnt.Payload.UpdateFromLastPakage)
            {
                lastPackageInfo.PackagesRequestInfo.Add(packageId, null);
            }

            return currentState;
        }

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<UnlinkUserFromDeviceCommand> evnt)
        {
            currentState.Users.Remove(evnt.Payload.UserId);
            currentState.LastUpdateDate = evnt.EventTimeStamp;
            return currentState;
        }

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<UserLinkingRequested> evnt)
        {
            currentState.Users.Add(evnt.Payload.UserId);
            currentState.LastUpdateDate = evnt.EventTimeStamp;
            return currentState;
        }

    }
}
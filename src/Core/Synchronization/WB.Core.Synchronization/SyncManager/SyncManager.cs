using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.View.CompleteQuestionnaire;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncManager
{
    using System;
    using SyncProvider;

    public class SyncManager  : ISyncManager
    {
        public SyncManager(ISyncProvider syncProvider)
        {
            this.syncProvider = syncProvider;
        }

        private readonly ISyncProvider syncProvider;

        public HandshakePackage ItitSync(ClientIdentifier identifier)
        {
            this.CheckAndCreateNewProcess(identifier);

            return new HandshakePackage(identifier.ClientInstanceKey);
        }

        private void CheckAndCreateNewProcess(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier.ClientInstanceKey == Guid.Empty)
                throw new ArgumentException("ClientInstanceKey is incorrecct.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientDeviceKey))
                throw new ArgumentException("ClientDeviceKey is incorrecct.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientVersionIdentifier))
                throw new ArgumentException("ClientVersionIdentifier is incorrecct.");
            
            //TODO: create new 

            throw new NotImplementedException();
        }

        public bool InitSending(ClientIdentifier identifier)
        {
            throw new NotImplementedException();
        }
        
        public bool SendSyncPackage(SyncPackage package)
        {
            throw new NotImplementedException();
        }

        public bool SendSyncItem(SyncItem item)
        {
            return syncProvider.HandleSyncItem(item);
        }

        public IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId)
        {
           return syncProvider.GetAllARIds(userId);
        }

        public bool ItitReceiving(ClientIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public SyncPackage ReceiveSyncPackage(ClientIdentifier identifier, Guid id, string itemType)
        {
            var syncPackage = new SyncPackage();

            var item = syncProvider.GetSyncItem(id, itemType);

            if (item != null)
            {
                syncPackage.ItemsContainer.Add(item);
                syncPackage.Status = true;
                syncPackage.Message = "OK";
            }
            else
            {
                syncPackage.Status = false;
                syncPackage.Message = "Item was not found";
            }

            return syncPackage;
        }

    }
}

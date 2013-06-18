using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

using System;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.SyncManager
{
    public class SyncManager : ISyncManager
    {
        private readonly ISyncProvider syncProvider;

        public SyncManager(ISyncProvider syncProvider)
        {
            this.syncProvider = syncProvider;
        }

        public HandshakePackage ItitSync(ClientIdentifier identifier)
        {
            return CheckAndCreateNewProcess(identifier);
        }

        public bool InitSending(ClientIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public bool SendSyncPackage(SyncPackage package)
        {
            ValidatePackage(package);

            foreach (var syncItem in package.ItemsContainer)
            {
                return syncProvider.HandleSyncItem(syncItem, package.SyncProcessKey);
            }

            return true;
        }

        private void ValidatePackage(SyncPackage package)
        {
            if(package == null)
                throw new ArgumentException("Package is not valid.");

            if(package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new ArgumentException("Package does'n contain correct content.");

            if (package.SyncProcessKey == Guid.Empty)
            {
                throw  new ArgumentException("Package doesn't contan valid sync process info.");
            }
        }

        public bool SendSyncItem(SyncItem item)
        {
            return syncProvider.HandleSyncItem(item, Guid.Empty);
        }

        public IEnumerable<SyncItemsMeta> GetAllARIds(Guid userId, Guid clientRegistrationKey)
        {
           return syncProvider.GetAllARIds(userId, clientRegistrationKey);
        }

        public bool InitReceiving(ClientIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public SyncPackage ReceiveSyncPackage(ClientIdentifier identifier, Guid id, string itemType)
        {
            var syncPackage = new SyncPackage();

            if (identifier == null)
                throw new ArgumentException("Client Identifier is not set.");

            if(!identifier.CurrentProcessKey.HasValue)
                throw new ArgumentException("Sync identifiier is not set.");

            SyncItem item = syncProvider.GetSyncItem(identifier.CurrentProcessKey.Value, id, itemType);
            
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

        public HandshakePackage CheckAndCreateNewProcess(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier.ClientInstanceKey == Guid.Empty)
                throw new ArgumentException("ClientInstanceKey is incorrecct.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientDeviceKey))
                throw new ArgumentException("ClientDeviceKey is incorrecct.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientVersionIdentifier))
                throw new ArgumentException("ClientVersionIdentifier is incorrecct.");

            return syncProvider.CheckAndCreateNewSyncActivity(clientIdentifier);
        }
    }
}
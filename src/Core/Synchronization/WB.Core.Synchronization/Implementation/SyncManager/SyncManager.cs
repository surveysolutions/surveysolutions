using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.Implementation.SyncManager
{
    internal class SyncManager : ISyncManager
    {
        private readonly ISyncProvider syncProvider;

        public SyncManager(ISyncProvider syncProvider)
        {
            this.syncProvider = syncProvider;
        }

        public HandshakePackage ItitSync(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier.ClientInstanceKey == Guid.Empty)
                throw new ArgumentException("ClientInstanceKey is incorrect.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientDeviceKey))
                throw new ArgumentException("ClientDeviceKey is incorrect.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.ClientVersionIdentifier))
                throw new ArgumentException("ClientVersionIdentifier is incorrect.");

            return this.syncProvider.CheckAndCreateNewSyncActivity(clientIdentifier);
        }

        public bool SendSyncPackage(SyncPackage package)
        {
            this.ValidatePackage(package);

            foreach (var syncItem in package.ItemsContainer)
            {
                return this.syncProvider.HandleSyncItem(syncItem, package.SyncProcessKey);
            }

            return true;
        }

        private void ValidatePackage(SyncPackage package)
        {
            if (package == null)
                throw new ArgumentException("Package is not valid.");

            if (package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new ArgumentException("Package doesn't contain correct content.");

            if (package.SyncProcessKey == Guid.Empty)
            {
                throw new ArgumentException("Package doesn't contain valid sync process info.");
            }
        }

        public bool SendSyncItem(SyncItem item)
        {
            return this.syncProvider.HandleSyncItem(item, Guid.Empty);
        }

        public IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, long clientSequence)
        {
            return this.syncProvider.GetAllARIdsWithOrder(userId, clientRegistrationKey, clientSequence);
        }


        public SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, Guid id, long sequence)
        {
            var syncPackage = new SyncPackage();

            SyncItem item = this.syncProvider.GetSyncItem(clientRegistrationId, id, sequence);

            if (item != null)
            {
                syncPackage.ItemsContainer.Add(item);
                syncPackage.IsErrorOccured = false;
            }
            else
            {
                syncPackage.IsErrorOccured = true;
                syncPackage.ErrorMessage = "Item was not found";
            }

            return syncPackage;
        }

        public SyncPackage ReceiveLastSyncPackage(Guid userId, Guid clientRegistrationId, long sequence)
        {
            var syncPackage = new SyncPackage();

            var items = this.syncProvider.GetSyncItemBulk(userId, clientRegistrationId, sequence);

            if (items != null)
            {
                syncPackage.ItemsContainer.AddRange(items);
                syncPackage.IsErrorOccured = false;
                //syncPackage.ErrorMessage = "OK";
            }
            else
            {
                syncPackage.IsErrorOccured = true;
                syncPackage.ErrorMessage = "Item was not found";
            }

            return syncPackage;
        }

        public int GetNumberToGet(Guid userId, Guid clientRegistrationId, long sequence)
        {
            return 0;
        }
    }
}
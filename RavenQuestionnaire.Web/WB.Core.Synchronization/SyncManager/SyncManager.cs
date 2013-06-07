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

        public bool InitSending(ClientIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public void SendSyncPackage()
        {
            throw new NotImplementedException();
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

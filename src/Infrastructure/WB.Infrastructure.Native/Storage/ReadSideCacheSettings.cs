namespace WB.Infrastructure.Native.Storage
{
    public class ReadSideCacheSettings
    {
        public ReadSideCacheSettings(int cacheSizeInEntities, int storeOperationBulkSize)
        {
            this.CacheSizeInEntities = cacheSizeInEntities;
            this.StoreOperationBulkSize = storeOperationBulkSize;
        }

        public int CacheSizeInEntities { get; private set; }
        public int StoreOperationBulkSize { get; private set; }
    }
}
namespace WB.Core.Infrastructure.Storage
{
    public class ReadSideCacheSettings
    {
        public ReadSideCacheSettings(string esentCacheFolder, int cacheSizeInEntities, int storeOperationBulkSize)
        {
            this.CacheSizeInEntities = cacheSizeInEntities;
            this.StoreOperationBulkSize = storeOperationBulkSize;
            this.EsentCacheFolder = esentCacheFolder;
        }

        public bool EnableEsentCache { get; } = true;

        public string EsentCacheFolder { get; private set; }
        public int CacheSizeInEntities { get; private set; }
        public int StoreOperationBulkSize { get; private set; }
    }
}
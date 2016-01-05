namespace WB.Infrastructure.Native.Storage
{
    public class ReadSideCacheSettings
    {
        public ReadSideCacheSettings(bool enableEsentCache, string esentCacheFolder, int cacheSizeInEntities, int storeOperationBulkSize)
        {
            this.EnableEsentCache = enableEsentCache;
            this.CacheSizeInEntities = cacheSizeInEntities;
            this.StoreOperationBulkSize = storeOperationBulkSize;
            this.EsentCacheFolder = esentCacheFolder;
        }

        public bool EnableEsentCache { get; private set; }
        public string EsentCacheFolder { get; private set; }
        public int CacheSizeInEntities { get; private set; }
        public int StoreOperationBulkSize { get; private set; }
    }
}
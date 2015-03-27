namespace WB.Core.Infrastructure.Storage.Memory.Implementation
{
    internal class ReadSideStoreMemoryCacheSettings
    {
        public ReadSideStoreMemoryCacheSettings(int maxCountOfCachedEntities, int maxCountOfEntitiesInOneStoreOperation)
        {
            this.MaxCountOfCachedEntities = maxCountOfCachedEntities;
            this.MaxCountOfEntitiesInOneStoreOperation = maxCountOfEntitiesInOneStoreOperation;
        }

        public int MaxCountOfCachedEntities { get; private set; }
        public int MaxCountOfEntitiesInOneStoreOperation { get; private set; }
    }
}
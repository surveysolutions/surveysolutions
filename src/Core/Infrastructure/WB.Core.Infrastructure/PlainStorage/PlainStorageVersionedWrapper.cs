namespace WB.Core.Infrastructure.PlainStorage
{
    public class PlainStorageVersionedWrapper<TEntity> where TEntity : class
    {
        private readonly IPlainStorageAccessor<TEntity> storage;

        public PlainStorageVersionedWrapper(IPlainStorageAccessor<TEntity> storage)
        {
            this.storage = storage;
        }

        public TEntity Get(string id, long version)
        {
            string versionedId = this.GetVersionedId(id, version);

            return this.storage.GetById(versionedId);
        }

        public void Remove(string id, long version)
        {
            string versionedId = this.GetVersionedId(id, version);

            this.storage.Remove(versionedId);
        }

        public void Store(TEntity view, string id, long version)
        {
            string versionedId = this.GetVersionedId(id, version);
            this.storage.Store(view, versionedId);
        }

        public string GetVersionedId(string id, long version)
        {
            return $"{id}${version}";
        }
    }
}

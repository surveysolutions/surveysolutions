using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public class ReadSideStorageVersionedWrapper<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> storage;

        public ReadSideStorageVersionedWrapper(IReadSideStorage<TEntity> storage)
        {
            this.storage = storage;
        }

        public TEntity Get(string id, long version)
        {
            string versionedId = GetVersionedId(id, version);

            return this.storage.GetById(versionedId);
        }

        public void Remove(string id, long version)
        {
            string versionedId = GetVersionedId(id, version);

            this.storage.Remove(versionedId);
        }

        public void Store(TEntity view, string id, long version)
        {
            string versionedId = GetVersionedId(id, version);
            this.storage.Store(view, versionedId);
        }

        public string GetVersionedId(string id, long version)
        {
            return string.Format("{0}${1}", id, version);
        }
    }
}

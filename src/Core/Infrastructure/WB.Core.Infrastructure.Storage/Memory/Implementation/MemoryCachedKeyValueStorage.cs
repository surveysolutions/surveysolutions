using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Memory.Implementation
{
    internal class MemoryCachedKeyValueStorage<TEntity> : MemoryCachedReadSideStore<TEntity>,
        IReadSideKeyValueStorage<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        public MemoryCachedKeyValueStorage(IReadSideKeyValueStorage<TEntity> readSideStorage)
            : base(readSideStorage)
        {
        }
    }
}

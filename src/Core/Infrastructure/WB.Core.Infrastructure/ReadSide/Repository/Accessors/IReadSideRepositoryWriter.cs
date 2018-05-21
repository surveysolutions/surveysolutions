using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    /// <summary>
    /// Accessor for read-side repository which should be used to update repository with new data from write-side.
    /// Please note, that reader (IReadSideRepositoryReader) should not be used to get data needed for such an update.
    /// All needed querying methods are already in this interface and should be used because they might be optimized for such operations.
    /// </summary>
    public interface IReadSideRepositoryWriter<TEntity> : IReadSideRepositoryWriter<TEntity, string>, IReadSideStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity {}

    /// <summary>
    /// Accessor for read-side repository which should be used to update repository with new data from write-side.
    /// Please note, that reader (IReadSideRepositoryReader) should not be used to get data needed for such an update.
    /// All needed querying methods are already in this interface and should be used because they might be optimized for such operations.
    /// </summary>
    public interface IReadSideRepositoryWriter<TEntity, TKey> : IReadSideStorage<TEntity, TKey>
        where TEntity : class, IReadSideRepositoryEntity {}
}

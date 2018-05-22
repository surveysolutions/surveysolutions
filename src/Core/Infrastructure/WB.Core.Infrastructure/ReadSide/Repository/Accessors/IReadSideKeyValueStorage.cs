using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    /// <summary>
    /// Accessor key/value only operations. Without any kind of queries.
    /// </summary>
    /// <remarks>
    /// Actually real key/value storage also support range queries by ID which are as fast as simple GetByID operations considering amount of returned data.
    /// However as far as we don't use real key/value and don't use ranges, this interface does not have such operations.
    /// -- TLK
    /// </remarks>
    public interface IReadSideKeyValueStorage<TEntity> : IReadSideStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity { }
    
    /// <summary>
    /// Accessor key/value only operations. Without any kind of queries.
    /// </summary>
    /// <remarks>
    /// Actually real key/value storage also support range queries by ID which are as fast as simple GetByID operations considering amount of returned data.
    /// However as far as we don't use real key/value and don't use ranges, this interface does not have such operations.
    /// -- TLK
    /// </remarks>
    public interface IReadSideKeyValueStorage<TEntity, TKey> : IReadSideStorage<TEntity, TKey>
        where TEntity : class, IReadSideRepositoryEntity
    { }
}

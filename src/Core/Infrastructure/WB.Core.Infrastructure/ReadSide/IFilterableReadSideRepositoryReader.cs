namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// Also supports filtering.
    /// </summary>
    public interface IFilterableReadSideRepositoryReader<TEntity> : IFilterableDenormalizerStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity, IView { }
}
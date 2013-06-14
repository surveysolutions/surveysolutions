namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// Also supports querying.
    /// </summary>
    public interface IQueryableReadSideRepositoryReader<TEntity> : IQueryableDenormalizerStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity, IView { }
}
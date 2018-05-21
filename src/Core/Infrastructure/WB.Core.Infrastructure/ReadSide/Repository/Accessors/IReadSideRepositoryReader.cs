using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// </summary>
    public interface IReadSideRepositoryReader<TEntity, TKey>
        where TEntity : class, IReadSideRepositoryEntity
    {
        int Count();

        TEntity GetById(TKey id);
    }

    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// </summary>
    public interface IReadSideRepositoryReader<TEntity> 
        where TEntity : class, IReadSideRepositoryEntity
    {
        int Count();

        TEntity GetById(string id);
    }
}

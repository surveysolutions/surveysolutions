using System;
using System.Linq;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// Also supports querying.
    /// </summary>
    public interface IQueryableReadSideRepositoryReader<TEntity> : IReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);
    }
}
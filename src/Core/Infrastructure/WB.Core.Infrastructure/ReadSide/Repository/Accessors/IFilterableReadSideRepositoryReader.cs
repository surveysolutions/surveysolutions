using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// Also supports filtering.
    /// </summary>
    public interface IFilterableReadSideRepositoryReader<TEntity> : IReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        IEnumerable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate);
    }
}
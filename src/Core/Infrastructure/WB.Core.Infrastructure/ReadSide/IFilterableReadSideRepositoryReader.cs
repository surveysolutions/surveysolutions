using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WB.Core.Infrastructure.ReadSide
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
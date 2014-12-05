using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    /// <summary>
    /// Accessor for read-side repository which should be used to update repository with new data from write-side.
    /// Please note, that reader (IReadSideRepositoryReader) should not be used to get data needed for such an update.
    /// All needed querying methods are already in this interface and should be used because they might be optimized for such operations.
    /// Also supports filtering.
    /// </summary>
    public interface IFilterableReadSideRepositoryWriter<TEntity> : IReadSideRepositoryWriter<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        IEnumerable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate);
    }
}
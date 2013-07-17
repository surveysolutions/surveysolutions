using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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


        #region aditional query functions

        int Count(Expression<Func<TEntity, bool>> query);
        IEnumerable<TEntity> QueryAll(Expression<Func<TEntity, bool>> query);
        
        IQueryable<TEntity> QueryEnumerable(Expression<Func<TEntity, bool>> query);

        #endregion

    }
}
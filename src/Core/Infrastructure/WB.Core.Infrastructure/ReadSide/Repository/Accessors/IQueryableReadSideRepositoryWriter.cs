using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IQueryableReadSideRepositoryWriter<TEntity> : IReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);

        #warning this method is here only because of our problems with Raven
        IEnumerable<TEntity> QueryAll(Expression<Func<TEntity, bool>> query);
    }
}

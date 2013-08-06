using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IQuerableReadSideRepositoryWriter<TEntity> : IReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);
    }
}

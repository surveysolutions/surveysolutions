using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IQueryablePlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity> where TEntity : class
    {
        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);
        IEnumerable<TEntity> LoadAll();
        void RemoveAll();
    }
}
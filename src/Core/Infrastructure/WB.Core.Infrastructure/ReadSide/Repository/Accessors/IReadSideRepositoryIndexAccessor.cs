using System;
using System.Linq;
using System.Reflection;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IReadSideRepositoryIndexAccessor
    {
        IQueryable<TResult> Query<TResult>(string indexName);

        TResult Query<TEntity, TResult>(string indexName, Func<IQueryable<TEntity>, TResult> query);

        void RegisterIndexesFromAssembly(Assembly assembly);
    }
}

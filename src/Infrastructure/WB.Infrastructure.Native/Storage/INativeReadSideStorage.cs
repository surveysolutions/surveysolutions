using System;
using System.Linq;
using NHibernate;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage
{
    public interface INativeReadSideStorage<TEntity>: IQueryableReadSideRepositoryReader<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        TResult QueryOver<TResult>(Func<IQueryOver<TEntity, TEntity>, TResult> query);
        int CountDistinctWithRecursiveIndex<TResult>(Func<IQueryOver<TEntity, TEntity>, IQueryOver<TResult, TResult>> query);
    }

    public interface INativeReadSideStorage<TEntity, TKey>: IQueryableReadSideRepositoryReader<TEntity, TKey> where TEntity : class, IReadSideRepositoryEntity
    {
        TResult QueryOver<TResult>(Func<IQueryOver<TEntity, TEntity>, TResult> query);
        int CountDistinctWithRecursiveIndex<TResult>(Func<IQueryOver<TEntity, TEntity>, IQueryOver<TResult, TResult>> query);
    }
}

using System;
using NHibernate;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage
{
    public interface INaviteReadSideStorage<TEntity>: IQueryableReadSideRepositoryReader<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        TResult QueryOver<TResult>(Func<IQueryOver<TEntity, TEntity>, TResult> query);
    }
}
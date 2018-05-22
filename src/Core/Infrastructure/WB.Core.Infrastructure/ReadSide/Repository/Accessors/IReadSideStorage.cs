using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IReadSideStorage
    {
        Type ViewType { get; }

        string GetReadableStatus();
    }

    public interface IReadSideStorage<TEntity> : IReadSideStorage<TEntity, string>
        where TEntity : class, IReadSideRepositoryEntity
    {
    }

    public interface IReadSideStorage<TEntity, TKey> : IReadSideStorage
        where TEntity : class, IReadSideRepositoryEntity
    {
        TEntity GetById(TKey id);

        void Remove(TKey id);

        void Store(TEntity view, TKey id);

        void BulkStore(List<Tuple<TEntity, TKey>> bulk);
        void Flush();
    }
}

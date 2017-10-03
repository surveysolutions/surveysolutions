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

    public interface IReadSideStorage<TEntity> : IReadSideStorage
        where TEntity : class, IReadSideRepositoryEntity
    {
        TEntity GetById(string id);

        void Remove(string id);

        void RemoveIfStartsWith(string beginingOfId);

        IEnumerable<string> GetIdsStartWith(string beginingOfId);

        void Store(TEntity view, string id);

        void BulkStore(List<Tuple<TEntity, string>> bulk);
    }
}
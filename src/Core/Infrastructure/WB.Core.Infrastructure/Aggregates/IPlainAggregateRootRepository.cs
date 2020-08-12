#nullable enable
using System;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IPlainAggregateRootRepository
    {
        T Get<T>(Guid aggregateId) where T : class, IPlainAggregateRoot;

        IPlainAggregateRoot? Get(Type aggregateRootType, Guid aggregateId);

        void Save(IPlainAggregateRoot aggregateRoot);
    }

    public interface IPlainAggregateRootRepository<T> where T : class, IPlainAggregateRoot
    {
        T? Get(Guid aggregateId);

        void Save(T aggregateRoot);
    }
}

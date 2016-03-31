using System;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRoot
    {
        void SetId(Guid id);
    }
}
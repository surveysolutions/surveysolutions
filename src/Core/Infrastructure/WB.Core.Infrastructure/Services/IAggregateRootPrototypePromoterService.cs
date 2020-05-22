using System;

namespace WB.Core.Infrastructure.Services
{
    public interface IAggregateRootPrototypePromoterService
    {
        void MaterializePrototypeIfRequired(Guid id);
    }
}

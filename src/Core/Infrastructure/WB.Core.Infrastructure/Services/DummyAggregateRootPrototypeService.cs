using System;

namespace WB.Core.Infrastructure.Services
{
    public class DummyAggregateRootPrototypeService : IAggregateRootPrototypeService
    {
        public PrototypeType? GetPrototypeType(Guid id)
        {
            return null;
        }

        public void MarkAsPrototype(Guid id, PrototypeType type)
        {
            
        }

        public void RemovePrototype(Guid id)
        {
            
        }
    }
}

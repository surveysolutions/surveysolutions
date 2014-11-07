using System;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain
{
    public abstract class AggregateRootMappedWithAttributes : MappedAggregateRoot
    {
        protected AggregateRootMappedWithAttributes()
            : base(new AttributeBasedEventHandlerMappingStrategy())
        {
        }

        protected AggregateRootMappedWithAttributes(Guid id)
            : base(id, new AttributeBasedEventHandlerMappingStrategy())
        {
        }
    }
}
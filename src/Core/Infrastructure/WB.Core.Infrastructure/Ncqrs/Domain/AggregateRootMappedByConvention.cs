using System;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain
{
    public abstract class AggregateRootMappedByConvention : MappedAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootMappedByConvention"/> class.
        /// </summary>
        protected AggregateRootMappedByConvention()
            : base(new ConventionBasedEventHandlerMappingStrategy())
        {
        }

        protected AggregateRootMappedByConvention(Guid id)
            : base(id, new ConventionBasedEventHandlerMappingStrategy())
        {
        }
    }
}

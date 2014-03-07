using Ncqrs.Domain;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates
{
    internal class Survey : AggregateRootMappedByConvention
    {
        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Survey() { }
    }
}
using System;
using System.Threading;
using System.Web.Caching;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterAggregateRootRepository : IEventSourcedAggregateRootRepository
    {
        private static Cache Cache => System.Web.HttpRuntime.Cache;

        public IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            var result = Cache[aggregateId.FormatGuid()];
            return (IEventSourcedAggregateRoot)result;
        }

        public IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress,
            CancellationToken cancellationToken)
        {
            var result = Cache[aggregateId.FormatGuid()];
            return (IEventSourcedAggregateRoot)result;
        }

        public IEventSourcedAggregateRoot GetStateless(Type aggregateType, Guid aggregateId)
        {
            var result = Cache[aggregateId.FormatGuid()];
            return (IEventSourcedAggregateRoot)result;
        }
    }
}
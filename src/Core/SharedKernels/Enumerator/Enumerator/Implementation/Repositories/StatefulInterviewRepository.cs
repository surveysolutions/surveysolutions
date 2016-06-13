using System;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefulInterviewRepository
    {
        private readonly IEventSourcedAggregateRootRepository aggregateRootRepository;
        private readonly ILiteEventBus eventBus;

        public StatefulInterviewRepository(IEventSourcedAggregateRootRepository aggregateRootRepository, ILiteEventBus eventBus)
        {
            if (aggregateRootRepository == null) throw new ArgumentNullException(nameof(aggregateRootRepository));
            if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));

            this.aggregateRootRepository = aggregateRootRepository;
            this.eventBus = eventBus;
        }

        public IStatefulInterview Get(string interviewId)
        {
            return GetImpl(interviewId,null, new CancellationToken());
        }

        public Task<IStatefulInterview> GetAsync(string interviewId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetImpl(interviewId, progress, cancellationToken));
        }

        public IStatefulInterview GetImpl(string interviewId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(interviewId)) throw new ArgumentNullException(nameof(interviewId));

            var interviewAggregateRootId = Guid.Parse(interviewId);

            var statefullInterview = (StatefulInterview)this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), interviewAggregateRootId, progress, cancellationToken);

            if (statefullInterview == null) return null;

            if (!statefullInterview.HasLinkedOptionsChangedEvents)
            {
                statefullInterview.MigrateLinkedOptionsToFiltered();
                this.eventBus.CommitUncommittedEvents(statefullInterview, null);
                statefullInterview.MarkChangesAsCommitted();
            }

            return statefullInterview;
        }
    }
}
using System;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefulInterviewRepository
    {
        private readonly IAggregateRootRepository aggregateRootRepository;
        private readonly ILiteEventBus eventBus;

        public StatefulInterviewRepository(IAggregateRootRepository aggregateRootRepository, ILiteEventBus eventBus)
        {
            if (aggregateRootRepository == null) throw new ArgumentNullException(nameof(aggregateRootRepository));
            if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));

            this.aggregateRootRepository = aggregateRootRepository;
            this.eventBus = eventBus;
        }

        public IStatefulInterview Get(string interviewId)
        {
            if (string.IsNullOrEmpty(interviewId)) throw new ArgumentNullException(nameof(interviewId));

            var interviewAggregateRootId = Guid.Parse(interviewId);
            var statefullInterview = (StatefulInterview) this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), interviewAggregateRootId);
            if (statefullInterview == null) throw new ArgumentOutOfRangeException(nameof(statefullInterview), $"Interview with Id: {interviewAggregateRootId} is not found.");

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
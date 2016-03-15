using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
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
            if (aggregateRootRepository == null) throw new ArgumentNullException("aggregateRootRepository");

            this.aggregateRootRepository = aggregateRootRepository;
            this.eventBus = eventBus;
        }

        public IStatefulInterview Get(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");

            var interviewAggregateRootId = Guid.Parse(interviewId);
            var statefullInterview = (StatefulInterview) this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), interviewAggregateRootId);

            if (!statefullInterview.HasLinkedOptionsChangedEvents)
            {
                statefullInterview.UpdateLinkedOptions();
                this.eventBus.CommitUncommittedEvents(statefullInterview, null);
                statefullInterview.MarkChangesAsCommitted();
            }

            return statefullInterview;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefulInterviewRepository
    {
        private readonly IAggregateRootRepository aggregateRootRepository;

        public StatefulInterviewRepository(IAggregateRootRepository aggregateRootRepository)
        {
            if (aggregateRootRepository == null) throw new ArgumentNullException("aggregateRootRepository");

            this.aggregateRootRepository = aggregateRootRepository;
        }

        public IStatefulInterview Get(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");

            var statefullInterview = (StatefulInterview) this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), Guid.Parse(interviewId));
            return statefullInterview;
        }
    }
}
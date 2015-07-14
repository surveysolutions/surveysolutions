using System;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Tester.Implementation.Repositories
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
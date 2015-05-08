using System;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefullInterviewRepository
    {
        private readonly IAggregateRootRepository aggregateRootRepository;

        public StatefulInterviewRepository(IAggregateRootRepository aggregateRootRepository)
        {
            if (aggregateRootRepository == null) throw new ArgumentNullException("aggregateRootRepository");

            this.aggregateRootRepository = aggregateRootRepository;
        }

        public IStatefulInterview Get(string interviewId)
        {
            var statefullInterview = (StatefulInterview) this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), Guid.Parse(interviewId));

            return statefullInterview;
        }
    }
}
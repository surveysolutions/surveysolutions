using System;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class StatefullInterviewRepository : IStatefullInterviewRepository
    {
        private readonly IAggregateRootRepository aggregateRootRepository;

        public StatefullInterviewRepository(IAggregateRootRepository aggregateRootRepository)
        {
            if (aggregateRootRepository == null) throw new ArgumentNullException("aggregateRootRepository");

            this.aggregateRootRepository = aggregateRootRepository;
        }

        public IStatefullInterview Get(string interviewId)
        {
            var statefullInterview = (StatefullInterview) this.aggregateRootRepository.GetLatest(typeof(StatefullInterview), Guid.Parse(interviewId));

            return statefullInterview;
        }
    }
}
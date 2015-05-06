using System;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefulInterviewRepository
    {
        private readonly IAggregateRootRepository aggregateRootRepository;

        public StatefulInterviewRepository(IAggregateRootRepository aggregateRootRepository)
        {
            if (aggregateRootRepository == null) throw new ArgumentNullException("aggregateRootRepository");

            this.aggregateRootRepository = aggregateRootRepository;
        }

        public InterviewModel Get(string interviewId)
        {
            var statefulInterview = (StatefullInterview) this.aggregateRootRepository.GetLatest(typeof(StatefullInterview), Guid.Parse(interviewId));

            return statefulInterview.InterviewModel;
        }
    }
}
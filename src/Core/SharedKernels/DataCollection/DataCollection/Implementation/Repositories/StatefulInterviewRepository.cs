using System;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
#nullable enable

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefulInterviewRepository
    {
        private readonly IEventSourcedAggregateRootRepository aggregateRootRepository;
        
        public StatefulInterviewRepository(IEventSourcedAggregateRootRepository aggregateRootRepository)
        {
            this.aggregateRootRepository = aggregateRootRepository ?? throw new ArgumentNullException(nameof(aggregateRootRepository));
        }

        public IStatefulInterview Get(string interviewId)
            => this.GetImpl(interviewId, null, CancellationToken.None);

        
        public async Task<IStatefulInterview> GetAsync(string interviewId, IProgress<EventReadingProgress> progress,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() => this.GetImpl(interviewId, progress, cancellationToken), cancellationToken);
        }

        public IStatefulInterview GetOrThrow(string interviewId)
            => this.GetImpl(interviewId, null, CancellationToken.None) ??
               throw new Exception("Interview not found")
               {
                   Data = {{"InterviewId", interviewId}}
               };

        private IStatefulInterview GetImpl(string interviewId, IProgress<EventReadingProgress>? progress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(interviewId)) throw new ArgumentNullException(nameof(interviewId));

            var interviewAggregateRootId = Guid.Parse(interviewId);

            var interview = (StatefulInterview)this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), interviewAggregateRootId, 
                progress, cancellationToken);
            
            return interview;
        }
    }
}

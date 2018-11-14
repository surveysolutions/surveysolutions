using System;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class StatefulInterviewRepository : IStatefulInterviewRepository
    {
        private readonly IEventSourcedAggregateRootRepository aggregateRootRepository;
        
        /*
         *IQuestionnaireStorage questionnaireRepository,
                                         IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider,
                                         ISubstitutionTextFactory substitutionTextFactory,
                                         IInterviewTreeBuilder treeBuilder,
                                         IQuestionOptionsRepository questionOptionsRepository
         *
         */

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

        private IStatefulInterview GetImpl(string interviewId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(interviewId)) throw new ArgumentNullException(nameof(interviewId));

            var interviewAggregateRootId = Guid.Parse(interviewId);

            var interview = (StatefulInterview)this.aggregateRootRepository.GetLatest(typeof(StatefulInterview), interviewAggregateRootId, 
                progress, cancellationToken);
            
            return interview;
        }
    }
}

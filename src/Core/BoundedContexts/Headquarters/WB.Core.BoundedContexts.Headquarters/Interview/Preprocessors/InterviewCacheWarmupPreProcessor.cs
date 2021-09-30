using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Preprocessors
{
    public class InterviewCacheWarmupPreProcessor : ICommandPreProcessor<StatefulInterview, InterviewCommand>
    {
        public void Process(StatefulInterview aggregate, InterviewCommand command)
        {
            if (aggregate.Version > 0)
            {
                aggregate.WarmUpCache();
            }
        }
    }
}

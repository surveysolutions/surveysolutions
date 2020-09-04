using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IWebInterviewTimezoneSetter : ICommandPreProcessor<StatefulInterview, InterviewCommand>
    {

    }
}

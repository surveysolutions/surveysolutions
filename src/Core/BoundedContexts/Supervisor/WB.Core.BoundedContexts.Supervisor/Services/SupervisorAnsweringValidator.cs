using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public class SupervisorAnsweringValidator : ICommandValidator<StatefulInterview, QuestionCommand>
    {
        public void Validate(StatefulInterview aggregate, QuestionCommand command)
        {
            if (aggregate.GetQuestion(command.Question).IsSupervisors)
                return;

            throw new AnswerNotAcceptedException(InterviewMessages.NotAllowedAnswеringNonSupervisor);
        }
    }
}

using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Validators
{
    public class InterviewReceivedByInterviewerCommandValidator : ICommandValidator<StatefulInterview, InterviewCommand>
    {
        public void Validate(StatefulInterview aggregate, InterviewCommand command)
        {
            if (aggregate.ReceivedByInterviewer && !IsAllowedCommand(command))
                throw new InterviewException(
                    WebInterviewResources.InterviewReceivedByInterviewer,
                    InterviewDomainExceptionType.InterviewRecievedByDevice)
                {
                    Data =
                    {
                        {InterviewQuestionInvariants.ExceptionKeys.InterviewId, aggregate.Id},
                    }
                };
        }

        private static bool IsAllowedCommand(InterviewCommand command)
        {
            return command is CommentAnswerCommand
                || command is ResolveCommentAnswerCommand
                || command is OpenInterviewBySupervisorCommand
                || command is CloseInterviewBySupervisorCommand
                || command is ResumeInterviewCommand
                || command is PauseInterviewCommand;
        }
    }
}

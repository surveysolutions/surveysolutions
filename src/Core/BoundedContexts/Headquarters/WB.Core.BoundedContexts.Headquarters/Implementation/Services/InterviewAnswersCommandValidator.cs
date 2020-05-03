using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class InterviewAnswersCommandValidator :
        ICommandValidator<StatefulInterview, AnswerNumericIntegerQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerNumericRealQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerSingleOptionQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerSingleOptionLinkedQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerMultipleOptionsQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerMultipleOptionsLinkedQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerYesNoQuestion>,
        ICommandValidator<StatefulInterview, AnswerDateTimeQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerGeoLocationQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerPictureQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerQRBarcodeQuestionCommand>,
        ICommandValidator<StatefulInterview, AnswerTextListQuestionCommand>
    {
        private readonly IAllInterviewsFactory interviewSummaryViewFactory;

        public InterviewAnswersCommandValidator(IAllInterviewsFactory interviewSummaryViewFactory)
        {
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
        }

        private void ThrowIfUserDontHavePermissionsToAnswer(StatefulInterview interview, InterviewCommand command)
        {
            var interviewSummary = this.interviewSummaryViewFactory.Load(interview.EventSourceId);
            if(command.UserId != interviewSummary.TeamLeadId)
                throw new InterviewException(CommandValidatorsMessages.UserDontHavePermissionsToAnswer);
        }

        public void Validate(StatefulInterview aggregate, AnswerNumericIntegerQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerNumericRealQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerSingleOptionQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerSingleOptionLinkedQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerMultipleOptionsQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerMultipleOptionsLinkedQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerYesNoQuestion command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerDateTimeQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerGeoLocationQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerPictureQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerQRBarcodeQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(StatefulInterview aggregate, AnswerTextListQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
    }
}

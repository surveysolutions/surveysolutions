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
        ICommandValidator<Interview, AnswerTextQuestionCommand>,
        ICommandValidator<Interview, AnswerNumericIntegerQuestionCommand>,
        ICommandValidator<Interview, AnswerNumericRealQuestionCommand>,
        ICommandValidator<Interview, AnswerSingleOptionQuestionCommand>,
        ICommandValidator<Interview, AnswerSingleOptionLinkedQuestionCommand>,
        ICommandValidator<Interview, AnswerMultipleOptionsQuestionCommand>,
        ICommandValidator<Interview, AnswerMultipleOptionsLinkedQuestionCommand>,
        ICommandValidator<Interview, AnswerYesNoQuestion>,
        ICommandValidator<Interview, AnswerDateTimeQuestionCommand>,
        ICommandValidator<Interview, AnswerGeoLocationQuestionCommand>,
        ICommandValidator<Interview, AnswerPictureQuestionCommand>,
        ICommandValidator<Interview, AnswerQRBarcodeQuestionCommand>,
        ICommandValidator<Interview, AnswerTextListQuestionCommand>
    {
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;

        public InterviewAnswersCommandValidator(IInterviewSummaryViewFactory interviewSummaryViewFactory)
        {
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
        }

        private void ThrowIfUserDontHavePermissionsToAnswer(Interview interview, InterviewCommand command)
        {
            var interviewSummary = this.interviewSummaryViewFactory.Load(interview.EventSourceId);
            if(command.UserId != interviewSummary.TeamLeadId)
                throw new InterviewException(CommandValidatorsMessages.UserDontHavePermissionsToAnswer);
        }

        public void Validate(Interview aggregate, AnswerTextQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerNumericIntegerQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerNumericRealQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerSingleOptionQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerSingleOptionLinkedQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerMultipleOptionsQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerMultipleOptionsLinkedQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerYesNoQuestion command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerDateTimeQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerGeoLocationQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerPictureQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerQRBarcodeQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerTextListQuestionCommand command) => this.ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
    }
}

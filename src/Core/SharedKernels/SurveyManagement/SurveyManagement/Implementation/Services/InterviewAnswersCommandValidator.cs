using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Properties;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
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

        public void Validate(Interview aggregate, AnswerTextQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerNumericIntegerQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerNumericRealQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerSingleOptionQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerSingleOptionLinkedQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerMultipleOptionsQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerMultipleOptionsLinkedQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerYesNoQuestion command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerDateTimeQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerGeoLocationQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerPictureQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerQRBarcodeQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
        public void Validate(Interview aggregate, AnswerTextListQuestionCommand command) => ThrowIfUserDontHavePermissionsToAnswer(aggregate, command);
    }
}

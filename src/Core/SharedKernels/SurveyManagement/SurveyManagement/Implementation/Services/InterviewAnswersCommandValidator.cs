using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Properties;

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
        private void ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(Interview interview)
        {
            if (interview.Status != InterviewStatus.ApprovedByHeadquarters) return;
            
            throw new InterviewException(CommandValidatorsMessages.InterviewApprovedByHQ);
        }

        public void Validate(Interview aggregate, AnswerTextQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerNumericIntegerQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerNumericRealQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerSingleOptionQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerSingleOptionLinkedQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerMultipleOptionsQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerMultipleOptionsLinkedQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerYesNoQuestion command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerDateTimeQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerGeoLocationQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerPictureQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerQRBarcodeQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }

        public void Validate(Interview aggregate, AnswerTextListQuestionCommand command)
        {
            this.ThrowIfAnswerOnQuestionAndInterviewApprovedByHQ(aggregate);
        }
    }
}

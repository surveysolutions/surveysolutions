using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewAnswersCommandValidatorTests
{
    [Subject(typeof(InterviewAnswersCommandValidator))]
    internal class when_responsible_supervisor_answer_on_supervisors_questions
    {
        Establish context = () =>
        {
            interview.Apply(Create.SupervisorAssignedEvent(interviewId: interviewId, supervisorId: responsibleId.FormatGuid()).Payload);
            commandValidator = Create.InterviewAnswersCommandValidator();
        };

        Because of = () => commandValidations.ForEach(validate => exceptions.Add(Catch.Only<InterviewException>(validate)));

        It should_not_any_interview_exceptions = () =>
            exceptions.ShouldEachConformTo(x => x == null);

        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Interview interview = Create.Interview(interviewId);

        private static readonly Action[] commandValidations =
        {
            () => commandValidator.Validate(interview, Create.AnswerDateTimeQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerTextQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerTextListQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerNumericRealQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerNumericIntegerQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerSingleOptionQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerSingleOptionLinkedQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerQRBarcodeQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerMultipleOptionsQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerMultipleOptionsLinkedQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerPictureQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.AnswerYesNoQuestion(interviewId: interviewId, userId: responsibleId, answer: new List<AnsweredYesNoOption>())),
            () => commandValidator.Validate(interview, Create.AnswerGeoLocationQuestionCommand(interviewId: interviewId, userId: responsibleId))
        };
        private static readonly List<InterviewException> exceptions = new List<InterviewException>();
        private static InterviewAnswersCommandValidator commandValidator;
    }
}
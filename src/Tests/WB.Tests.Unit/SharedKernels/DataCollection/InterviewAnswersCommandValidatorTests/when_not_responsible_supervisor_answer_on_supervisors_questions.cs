using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Properties;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewAnswersCommandValidatorTests
{
    [Subject(typeof(InterviewAnswersCommandValidator))]
    internal class when_not_responsible_supervisor_answer_on_supervisors_questions
    {
        Establish context = () =>
        {
            commandValidator = Create.InterviewAnswersCommandValidator();
        };

        Because of = () => commandValidations.ForEach(validate => exceptions.Add(Catch.Only<InterviewException>(validate)));

        It should_exceptions_have_specified_error_messages = () =>
            exceptions.ShouldEachConformTo(x => x.Message == CommandValidatorsMessages.UserDontHavePermissionsToAnswer);

        It should_number_of_raised_interviewExceptions_be_equal_to_number_of_commands = () =>
            exceptions.Count.ShouldEqual(commandValidations.Length);

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
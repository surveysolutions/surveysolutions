using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_all_type_of_questions_and_interview_approved_by_hq : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Other.TextListQuestion(questionId: questionId));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.ApprovedByHeadquarters).Payload);
        };

        Because of = () =>
            answerCommands.ForEach(answerCommand => exceptions.Add(Catch.Only<InterviewException>(answerCommand)));

        It should_exceptions_have_specified_error_messages = () =>
            exceptions.ShouldEachConformTo(exception => new []{"interview", "approved"}.All(keyword => exception.Message.ToLower().Contains(keyword)) );

        It should_number_of_raised_interviewExceptions_be_equal_to_number_of_commands = () =>
            exceptions.All(x => x != null).ShouldBeTrue();

        private static Interview interview;
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid questionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly List<Action> answerCommands = new List<Action>()
        {
            () => interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, new Tuple<decimal, string>[0]),
            () => interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, ""),
            () => interview.AnswerDateTimeQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, DateTime.UtcNow),
            () => interview.AnswerGeoLocationQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 0, 0, 0, 0, DateTimeOffset.UtcNow),
            () => interview.AnswerMultipleOptionsQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, new decimal[0]),
            () => interview.AnswerMultipleOptionsLinkedQuestion(userId, questionId,RosterVector.Empty, DateTime.Now, new [] { new decimal[0] }),
            () => interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 0),
            () => interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 0),
            () => interview.AnswerPictureQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, string.Empty),
            () => interview.AnswerQRBarcodeQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, string.Empty),
            () => interview.AnswerSingleOptionQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 1),
            () => interview.AnswerSingleOptionLinkedQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, new decimal[0]),
            () => interview.AnswerYesNoQuestion(new AnswerYesNoQuestion(interview.EventSourceId, userId, questionId, RosterVector.Empty, DateTime.UtcNow, new AnsweredYesNoOption[0])),
        };
        
        private static readonly List<InterviewException> exceptions = new List<InterviewException>();
    }
}
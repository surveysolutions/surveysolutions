using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_numeric_real_question_received_by_interviewer : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericQuestion(questionId: questionId, isInteger: false));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewReceivedByInterviewer());
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() => interview.AnswerNumericRealQuestion(userId, questionId, new decimal[0], DateTime.Now, 1.11m));

        It should_throw_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_throw_InterviewException_with_explanation = () =>
            exception.Message.ShouldEqual($"Can't modify Interview {interview.EventSourceId.FormatGuid()} on server, because it received by interviewer.");

        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static InterviewException exception;
    }
}
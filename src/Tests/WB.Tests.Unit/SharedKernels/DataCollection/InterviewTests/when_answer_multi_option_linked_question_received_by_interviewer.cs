using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_multi_option_linked_question_received_by_interviewer : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionId = Guid.Parse("33333333333333333333333333333333");
            var textQuestionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId: textQuestionId),
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, linkedToQuestionId: textQuestionId));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewReceivedByInterviewer());
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  Assert.Throws<InterviewException>(
                () => interview.AnswerMultipleOptionsLinkedQuestion(userId, questionId, new decimal[0], DateTime.Now, new RosterVector[] {}));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_InterviewException_with_explanation () =>
            exception.Message.Should().Be($"Can't modify Interview {interview.EventSourceId.FormatGuid()} on server, because it received by interviewer.");

        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static InterviewException exception;
    }
}

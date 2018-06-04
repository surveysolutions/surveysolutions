using System;
using System.Linq;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_yes_no_question_received_by_interviewer : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId: questionId));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewReceivedByInterviewer(DateTimeOffset.Now));
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() => 
                interview.AnswerYesNoQuestion(new AnswerYesNoQuestion(interview.EventSourceId, userId, questionId,  new decimal[0], Enumerable.Empty<AnsweredYesNoOption>())));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_InterviewException_with_explanation () =>
            exception.Message.Should().Be($"Can't modify Interview on server, because it received by interviewer");

        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static InterviewException exception;
    }
}

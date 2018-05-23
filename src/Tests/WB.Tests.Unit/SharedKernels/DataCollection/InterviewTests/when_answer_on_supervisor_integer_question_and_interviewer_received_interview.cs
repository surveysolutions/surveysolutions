using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_supervisor_integer_question_and_interviewer_received_interview : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            questionId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => _.HasQuestion(questionId) == true
                                                        && _.GetQuestionType(questionId) == QuestionType.Numeric
                                                        && _.IsQuestionInteger(questionId) == true
                                                        && _.GetQuestionScope(questionId) == QuestionScope.Supervisor);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewReceivedByInterviewer());
            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            exception = NUnit.Framework.Assert.Throws<InterviewException>(() => interview.AnswerNumericIntegerQuestion(userId, questionId, new decimal[] { }, DateTime.Now, 0));

        [NUnit.Framework.Test] public void should_raise_InterviewException () 
        {
            exception.Should().NotBeNull();
            exception.Should().BeOfType<InterviewException>();
            exception.Message.Should().Be($"Can't modify Interview on server, because it received by interviewer");
        }

        [NUnit.Framework.Test] public void should_not_raise_any_NumericIntegerQuestionAnswered_event () =>
            eventContext.ShouldNotContainEvent<NumericIntegerQuestionAnswered>();

        [NUnit.Framework.Test] public void should_not_raise_any_events () =>
            eventContext.Events.Should().BeEmpty();

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static Exception exception;
    }
}

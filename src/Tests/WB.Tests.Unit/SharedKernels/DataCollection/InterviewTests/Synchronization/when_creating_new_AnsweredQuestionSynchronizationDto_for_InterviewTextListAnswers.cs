using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_creating_new_AnsweredQuestionSynchronizationDto_for_InterviewTextListAnswers : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("11000000000000000000000000000000");
            textListAnswer = new[] { Tuple.Create((decimal) 1, "one"), Tuple.Create((decimal) 2, "two") };
            interviewTextListAnswers = new InterviewTextListAnswers(textListAnswer);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Create.Entity.AnsweredQuestionSynchronizationDto(questionId, EmptyRosterVector, interviewTextListAnswers);

        [NUnit.Framework.Test] public void should_answer_value_be_equal_to_provided_answer_value () =>
            result.Answer.Should().BeEquivalentTo(textListAnswer);

        private static Guid questionId;
        private static InterviewTextListAnswers interviewTextListAnswers;
        private static Tuple<decimal, string>[] textListAnswer;
        private static readonly decimal[] EmptyRosterVector = new decimal[]{};

        private static AnsweredQuestionSynchronizationDto result;
    }
}

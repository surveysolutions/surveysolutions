using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_creating_new_AnsweredQuestionSynchronizationDto_for_InterviewTextListAnswers : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("11000000000000000000000000000000");
            textListAnswer = new[] { Tuple.Create((decimal) 1, "one"), Tuple.Create((decimal) 2, "two") };
            interviewTextListAnswers = new InterviewTextListAnswers(textListAnswer);
        };

        Because of = () =>
            result = Create.Entity.AnsweredQuestionSynchronizationDto(questionId, EmptyRosterVector, interviewTextListAnswers);

        It should_answer_value_be_equal_to_provided_answer_value = () =>
            result.Answer.ShouldEqual(textListAnswer);

        private static Guid questionId;
        private static InterviewTextListAnswers interviewTextListAnswers;
        private static Tuple<decimal, string>[] textListAnswer;
        private static readonly decimal[] EmptyRosterVector = new decimal[]{};

        private static AnsweredQuestionSynchronizationDto result;
    }
}
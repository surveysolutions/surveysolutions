using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    public class when_getting_question_answer : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            interview = new StatefulInterview();

            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var targetRosterVector = new[] {0m};

            interview.Apply(Create.Event.TextQuestionAnswered(questionId, targetRosterVector, "answer"));
        };

        Because of = () => answerValue = interview.GetAnswer(questionId, new decimal[]{0m, 1m});

        It should_reduce_roster_vector_to_find_target_question_answer = () => ((MaskedTextAnswer)answerValue).Answer.ShouldEqual("answer");

        private static StatefulInterview interview;
        private static Guid questionId;
        private static BaseInterviewAnswer answerValue;
    }
}


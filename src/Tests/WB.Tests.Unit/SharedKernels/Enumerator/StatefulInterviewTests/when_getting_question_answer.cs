using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_question_answer : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var targetRosterVector = new[] {0m};

            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetRosterLevelForQuestion(questionId) == 1);
            IQuestionnaireStorage questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, targetRosterVector, "answer"));
        };

        Because of = () => answerValue = interview.FindBaseAnswerByOrDeeperRosterLevel(questionId, new decimal[]{0m, 1m});

        It should_reduce_roster_vector_to_find_target_question_answer = () => ((TextAnswer)answerValue).Answer.ShouldEqual("answer");

        private static StatefulInterview interview;
        private static Guid questionId;
        private static BaseInterviewAnswer answerValue;
        private static Guid questionnaireId;
    }
}


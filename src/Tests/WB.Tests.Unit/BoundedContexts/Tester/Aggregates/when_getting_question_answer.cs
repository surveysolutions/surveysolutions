using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    public class when_getting_question_answer : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);

            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var targetRosterVector = new[] {0m};

            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetRosterLevelForQuestion(questionId) == 1);
            IQuestionnaireRepository questionnaireRepository = Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire); 

            Setup.InstanceToMockedServiceLocator(questionnaireRepository);

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


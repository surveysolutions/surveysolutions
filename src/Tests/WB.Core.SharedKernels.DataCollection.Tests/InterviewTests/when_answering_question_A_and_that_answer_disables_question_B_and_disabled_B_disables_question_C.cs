using System;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_question_C : InterviewTestsContext
    {
        Establish context = () =>
        {
            emptyRosterVector = new int[]{};
            userId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");

            questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            string questionBEnablementCondition = "disable B on demand please";
            string questionCEnablementCondition = "disable C on demand please";


            Expression<Func<Guid, bool>> abcQuestionId = id => id == questionAId || id == questionBId || id == questionCId;

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(it.Is(abcQuestionId)) == true
                && _.GetQuestionType(it.Is(abcQuestionId)) == QuestionType.Text
                && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new [] { questionBId }
                && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBId) == new [] { questionCId }
                && _.GetCustomEnablementConditionForQuestion(questionBId) == questionBEnablementCondition
                && _.GetCustomEnablementConditionForQuestion(questionCId) == questionCEnablementCondition
            );

            var expressionProcessor = Mock.Of<IExpressionProcessor>
            (_
                => _.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>()) == true
                && _.EvaluateBooleanExpression(questionBEnablementCondition, it.IsAny<Func<string, object>>()) == false
                && _.EvaluateBooleanExpression(questionCEnablementCondition, it.IsAny<Func<string, object>>()) == false
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire));
            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerTextQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, "disable B please");

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionDisabled_event_for_question_B = () =>
            eventContext.ShouldContainEvent<QuestionDisabled>(@event
                => @event.QuestionId == questionBId);

        It should_raise_QuestionDisabled_event_for_question_C = () =>
            eventContext.ShouldContainEvent<QuestionDisabled>(@event
                => @event.QuestionId == questionCId);

        private static EventContext eventContext;
        private static Guid questionAId;
        private static Guid questionBId;
        private static Guid questionCId;
        private static Interview interview;
        private static Guid userId;
        private static int[] emptyRosterVector;
    }
}
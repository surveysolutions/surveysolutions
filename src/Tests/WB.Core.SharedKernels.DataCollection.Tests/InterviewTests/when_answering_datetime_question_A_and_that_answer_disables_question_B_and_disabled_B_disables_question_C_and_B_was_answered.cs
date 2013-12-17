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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_datetime_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            emptyRosterVector = new decimal[]{};
            userId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");

            questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            string questionAVariableName = "a";
            questionBVariableName = "b";

            string questionBEnablementCondition = "disable B on demand please";
            questionCEnablementCondition = "disable C on demand please";


            Expression<Func<Guid, bool>> abcQuestionId = id => id == questionAId || id == questionBId || id == questionCId;

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(it.Is(abcQuestionId)) == true
                && _.GetQuestionType(it.Is(abcQuestionId)) == QuestionType.DateTime
                && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new [] { questionBId }
                && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBId) == new [] { questionCId }
                && _.GetCustomEnablementConditionForQuestion(questionBId) == questionBEnablementCondition
                && _.GetCustomEnablementConditionForQuestion(questionCId) == questionCEnablementCondition
                && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionBId) == new [] { new QuestionIdAndVariableName(questionAId, questionAVariableName) }
                && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionCId) == new[] { new QuestionIdAndVariableName(questionBId, questionBVariableName) }
            );

            expressionProcessor = Mock.Of<IExpressionProcessor>
            (_
                => _.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>()) == true
                && _.EvaluateBooleanExpression(questionBEnablementCondition, it.IsAny<Func<string, object>>()) == false
            );

            Mock.Get(expressionProcessor)
                .Setup(_ => _.EvaluateBooleanExpression(questionCEnablementCondition, it.IsAny<Func<string, object>>()))
                .Callback<string, Func<string, object>>((expression, getValueForIdentifier) =>
                    funcSuppliedWhenEvaluatingQuestionCEnablementCondition = getValueForIdentifier)
                .Returns(false);


            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire));
            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new DateTimeQuestionAnswered(userId, questionBId, emptyRosterVector, DateTime.Now, new DateTime(2012, 1, 1)));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerDateTimeQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, new DateTime(2014, 11, 11));

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

        It should_supply_null_answer_for_question_B_when_evaluating_question_C_disablement_condition = () =>
            funcSuppliedWhenEvaluatingQuestionCEnablementCondition.Invoke(questionBVariableName).ShouldEqual(null);

        private static EventContext eventContext;
        private static Guid questionAId;
        private static Guid questionBId;
        private static Guid questionCId;
        private static Interview interview;
        private static Guid userId;
        private static decimal[] emptyRosterVector;
        private static IExpressionProcessor expressionProcessor;
        private static string questionCEnablementCondition;
        private static Func<string, object> funcSuppliedWhenEvaluatingQuestionCEnablementCondition;
        private static string questionBVariableName;
    }
}
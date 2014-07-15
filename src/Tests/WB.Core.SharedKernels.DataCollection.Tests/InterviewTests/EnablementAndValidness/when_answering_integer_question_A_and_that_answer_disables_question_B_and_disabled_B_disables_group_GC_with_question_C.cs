using System;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.EnablementAndValidness
{
    [Ignore("C#")]
    internal class when_answering_integer_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_group_GC_with_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            emptyRosterVector = new decimal[] { };
            userId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");

            questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupGCId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            string questionAVariableName = "a";
            questionBVariableName = "b";

            string groupGCEnablementCondition = "disable Group C on demand please";
            questionBEnablementCondition = "disable B on demand please";


            Expression<Func<Guid, bool>> abcQuestionId = id => id == questionAId || id == questionBId || id == questionCId;

            var questionaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(Moq.It.Is(abcQuestionId)) == true
                        && _.GetQuestionType(Moq.It.Is(abcQuestionId)) == QuestionType.Numeric
                        && _.IsQuestionInteger(Moq.It.Is(abcQuestionId)) == true

                        && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new[] { questionBId }

                        && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBId) == new[] { groupGCId }

                        && _.GetAllUnderlyingQuestions(groupGCId) == new[] { questionCId }

                        && _.GetCustomEnablementConditionForGroup(groupGCId) == groupGCEnablementCondition
                        && _.GetCustomEnablementConditionForQuestion(questionBId) == questionBEnablementCondition
                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(groupGCId) == new[] { questionBId }

                        && _.GetQuestionVariableName(questionAId) == questionAVariableName
                        && _.GetQuestionVariableName(questionBId) == questionBVariableName

                        && _.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(groupGCId) == new[] { questionCId }
                );

            expressionProcessor = Mock.Of<IExpressionProcessor>
                (_
                    => _.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()) == true
                        && _.EvaluateBooleanExpression(questionBEnablementCondition, Moq.It.IsAny<Func<string, object>>()) == false
                );

            Mock.Get(expressionProcessor)
                .Setup(_ => _.EvaluateBooleanExpression(groupGCEnablementCondition, Moq.It.IsAny<Func<string, object>>()))
                .Callback<string, Func<string, object>>((expression, getValueForIdentifier) =>
                    funcSuppliedWhenEvaluatingGroupGCEnablementCondition = getValueForIdentifier)
                .Returns(false);


            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire));
            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionBId, emptyRosterVector, DateTime.Now, 42));
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerNumericIntegerQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, +100500);

        It should_disable_question_B = () => 
            eventContext.ShouldContainEvent<QuestionsDisabled>(@event => @event.Questions.Any(question => question.Id == questionBId));

        It should_raise_QuestionsDisabled_event_for_group_GB = () =>
            eventContext.ShouldContainEvent<GroupsDisabled>(@event => @event.Groups.Any(group => group.Id == groupGCId));

        It should_supply_null_answer_for_question_B_when_evaluating_group_GC_disablement_condition = () =>
            funcSuppliedWhenEvaluatingGroupGCEnablementCondition.Invoke(questionBVariableName).ShouldEqual(null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        static EventContext eventContext;
        static Guid questionAId;
        static Guid questionBId;
        static Guid questionCId;
        static Interview interview;
        static Guid userId;
        static decimal[] emptyRosterVector;
        static IExpressionProcessor expressionProcessor;
        static string questionBEnablementCondition;
        static Func<string, object> funcSuppliedWhenEvaluatingGroupGCEnablementCondition;
        static string questionBVariableName;
        static Guid groupGCId;
    }
}
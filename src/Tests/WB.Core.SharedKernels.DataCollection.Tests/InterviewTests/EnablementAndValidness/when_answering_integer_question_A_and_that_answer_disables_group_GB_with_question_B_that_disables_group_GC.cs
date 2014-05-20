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
    internal class when_answering_integer_question_A_and_that_answer_disables_group_GB_with_question_B_that_disables_group_GC : InterviewTestsContext
    {
        Establish context = () =>
        {
            emptyRosterVector = new decimal[] { };
            userId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");

            questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            groupGBId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupGCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            string questionAVariableName = "a";
            questionBVariableName = "b";

            string groupGBEnablementCondition = "disable B on demand please";

            Expression<Func<Guid, bool>> abQuestionId = id => id == questionAId || id == questionBId;

            string groupGCEnablementCondition = "disable GC";
            var questionaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(Moq.It.Is(abQuestionId)) == true
                        && _.GetQuestionType(Moq.It.Is(abQuestionId)) == QuestionType.Numeric
                        && _.IsQuestionInteger(Moq.It.Is(abQuestionId)) == true
                        && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new[] { groupGBId }
                        && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBId) == new[] { groupGCId }

                        && _.GetAllUnderlyingQuestions(groupGBId) == new[] { questionBId }

                        && _.GetCustomEnablementConditionForGroup(groupGBId) == groupGBEnablementCondition
                        && _.GetCustomEnablementConditionForGroup(groupGCId) == groupGCEnablementCondition
                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(groupGBId) == new[] { questionAId }
                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(groupGCId) == new[] { questionBId }

                        && _.GetQuestionVariableName(questionAId) == questionAVariableName
                        && _.GetQuestionVariableName(questionBId) == questionBVariableName

                        && _.GetAllParentGroupsForQuestion(questionBId) == new[] { groupGBId }
                );

            expressionProcessor = Mock.Of<IExpressionProcessor>
                (_
                    => _.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()) == true
                        && _.EvaluateBooleanExpression(groupGBEnablementCondition, Moq.It.IsAny<Func<string, object>>()) == false
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

        It should_raise_GroupsDisabled_event_for_group_GB = () =>
            eventContext.ShouldContainEvent<GroupsDisabled>(@event => @event.Groups.Any(group => group.Id == groupGBId));

        It should_raise_GroupsDisabled_event_for_group_GC = () =>
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
        static Interview interview;
        static Guid userId;
        static decimal[] emptyRosterVector;
        static IExpressionProcessor expressionProcessor;
        static Func<string, object> funcSuppliedWhenEvaluatingGroupGCEnablementCondition;
        static string questionBVariableName;
        static Guid groupGBId;
        private static Guid groupGCId;
    }
}
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
    internal class when_answering_integer_question_A_and_that_answer_disables_group_GB_with_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            emptyRosterVector = new decimal[] { };
            userId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");

            questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupGBId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            string questionAVariableName = "a";
            questionBVariableName = "b";

            string groupGBEnablementCondition = "disable B on demand please";
            questionCEnablementCondition = "disable C on demand please";


            Expression<Func<Guid, bool>> abcQuestionId = id => id == questionAId || id == questionBId || id == questionCId;

            var questionaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(Moq.It.Is(abcQuestionId)) == true
                        && _.GetQuestionType(Moq.It.Is(abcQuestionId)) == QuestionType.Numeric
                        && _.IsQuestionInteger(Moq.It.Is(abcQuestionId)) == true
                        && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new[] { questionBId }
                        && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBId) == new[] { questionCId }

                        && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new[] { groupGBId }
                        && _.GetAllUnderlyingQuestions(groupGBId) == new[] { questionBId }

                        && _.GetCustomEnablementConditionForGroup(groupGBId) == groupGBEnablementCondition
                        && _.GetCustomEnablementConditionForQuestion(questionCId) == questionCEnablementCondition
                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(groupGBId) == new[] { questionAId }
                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionCId) == new[] { questionBId }

                        && _.GetQuestionVariableName(questionAId) == questionAVariableName
                        && _.GetQuestionVariableName(questionBId) == questionBVariableName

                        && _.GetAllParentGroupsForQuestion(questionBId) == new[] { groupGBId }
                        && _.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(groupGBId) == new[] { questionBId }
                ); 

            expressionProcessor = Mock.Of<IExpressionProcessor>
                (_
                    => _.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()) == true
                        && _.EvaluateBooleanExpression(groupGBEnablementCondition, Moq.It.IsAny<Func<string, object>>()) == false
                );

            Mock.Get(expressionProcessor)
                .Setup(_ => _.EvaluateBooleanExpression(questionCEnablementCondition, Moq.It.IsAny<Func<string, object>>()))
                .Callback<string, Func<string, object>>((expression, getValueForIdentifier) =>
                    funcSuppliedWhenEvaluatingQuestionCEnablementCondition = getValueForIdentifier)
                .Returns(false);


            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire));
            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionBId, emptyRosterVector, DateTime.Now, 42));
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerNumericIntegerQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, +100500);

        It should_raise_QuestionsDisabled_event_for_group_GB = () =>
            eventContext.ShouldContainEvent<GroupsDisabled>(@event=> @event.Groups.Any(group => group.Id == groupGBId));

        It should_raise_QuestionsDisabled_event_for_question_C = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>(@event => @event.Questions.Any(question => question.Id == questionCId));

        It should_supply_null_answer_for_question_B_when_evaluating_question_C_disablement_condition = () =>
          funcSuppliedWhenEvaluatingQuestionCEnablementCondition.Invoke(questionBVariableName).ShouldEqual(null);

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
        static string questionCEnablementCondition;
        static Func<string, object> funcSuppliedWhenEvaluatingQuestionCEnablementCondition;
        static string questionBVariableName;
        static Guid groupGBId;
    }
}
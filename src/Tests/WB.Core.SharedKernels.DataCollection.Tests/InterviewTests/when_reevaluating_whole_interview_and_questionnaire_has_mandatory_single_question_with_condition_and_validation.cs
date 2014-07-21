using System;
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

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_reevaluating_whole_interview_and_questionnaire_has_mandatory_single_question_with_condition_and_validation : InterviewTestsContext
    {
        Establish context = () =>
        {
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var validationExpression = "[s00]!=1";
            var enablementCondition = "[s00]==2";

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(singleQuestion1Id) == true
                && _.HasQuestion(singleQuestion2Id) == true 
                && _.GetQuestionType(singleQuestion1Id) == QuestionType.SingleOption
                && _.GetQuestionType(singleQuestion2Id) == QuestionType.SingleOption
                && _.GetAnswerOptionsAsValues(singleQuestion1Id) == new decimal[] { 1, 2, 3 }
                && _.GetAnswerOptionsAsValues(singleQuestion2Id) == new decimal[] { 1, 2, 3 }
                && _.GetAllMandatoryQuestions() == new[] { singleQuestion1Id, singleQuestion2Id }
                && _.IsCustomValidationDefined(singleQuestion2Id) == true
                && _.GetAllQuestionsWithNotEmptyValidationExpressions() == new[] { singleQuestion2Id }
                && _.GetCustomValidationExpression(singleQuestion2Id) == validationExpression
                && _.GetAllQuestionsWithNotEmptyCustomEnablementConditions() == new[] { singleQuestion2Id }
                && _.GetCustomEnablementConditionForQuestion(singleQuestion2Id) == enablementCondition
                && _.IsQuestionMandatory(singleQuestion1Id) == true
                && _.IsQuestionMandatory(singleQuestion2Id) == true
            );

            var expressionProcessor =
                Mock.Of<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>(
                    x =>
                        x.EvaluateBooleanExpression(validationExpression, Moq.It.IsAny<Func<string, object>>()) == true &&
                        x.EvaluateBooleanExpression(enablementCondition, Moq.It.IsAny<Func<string, object>>()) == true &&
                        x.GetIdentifiersUsedInExpression(validationExpression) == new []{ singleQuestion1Id.ToString()} &&
                        x.GetIdentifiersUsedInExpression(enablementCondition) == new[] { singleQuestion1Id.ToString() });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepository);

            SetupInstanceToMockedServiceLocator<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new SingleOptionQuestionAnswered(userId: userId, questionId: singleQuestion1Id,
                propagationVector: new decimal[0], answerTime: DateTime.Now, selectedValue: 2.0m));
            interview.Apply(new SingleOptionQuestionAnswered(userId: userId, questionId: singleQuestion2Id,
                propagationVector: new decimal[0], answerTime: DateTime.Now, selectedValue: 1.0m));


            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_not_raise_AnswersDeclaredInvalid_event_with_single_QuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredInvalid>();

        It should_raise_AnswersDeclaredValid_event_with_questions_that_constains_singleQuestion1Id = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event => @event.Questions[0].Id == singleQuestion2Id);

        It should_raise_AnswersDeclaredValid_event_with_questions_that_constains_singleQuestion2Id = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event => @event.Questions[1].Id == singleQuestion1Id);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid singleQuestion1Id = Guid.Parse("20000000000000000000000000000000");
        private static Guid singleQuestion2Id = Guid.Parse("30000000000000000000000000000000");
    }
}
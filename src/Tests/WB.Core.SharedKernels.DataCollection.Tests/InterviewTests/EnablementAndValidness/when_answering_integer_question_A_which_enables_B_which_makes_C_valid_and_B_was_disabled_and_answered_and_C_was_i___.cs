using System;
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
using Identity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.EnablementAndValidness
{
    [Ignore("C#")]
    internal class when_answering_integer_question_A_which_enables_B_which_makes_C_valid_and_B_was_disabled_and_answered_and_C_was_invalid_and_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("77778888000000000000000000000000");

            Expression<Func<Guid, bool>> abcQuestionId = id => id == questionAId || id == questionBId || id == questionCId;

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(it.Is(abcQuestionId)) == true
                && _.GetQuestionType(it.Is(abcQuestionId)) == QuestionType.Numeric
                && _.IsQuestionInteger(it.Is(abcQuestionId)) == true

                && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionAId) == new[] { questionBId }
                && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionBId) == new[] { questionAId }

                && _.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(questionBId) == new[] { questionCId }
                && _.GetQuestionsInvolvedInCustomValidation(questionCId) == new[] { questionBId }

                && _.GetQuestionVariableName(questionAId) == "a"
                && _.GetQuestionVariableName(questionBId) == "b"
            );

            var expressionProcessor = Mock.Of<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>
            (_
                => _.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>()) == true
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire));
            SetupInstanceToMockedServiceLocator<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionBId, emptyRosterVector, DateTime.Now, 4400));
            interview.Apply(new QuestionsDisabled(new[] { new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(questionBId, emptyRosterVector) }));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionCId, emptyRosterVector, DateTime.Now, 42));
            interview.Apply(new AnswersDeclaredInvalid(new[] { new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(questionCId, emptyRosterVector) }));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, +100500);

        It should_raise_AnswersDeclaredValid_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>();

        It should_raise_AnswersDeclaredValid_event_with_Questions_containing_question_C = () =>
            eventContext.GetSingleEvent<AnswersDeclaredValid>()
                .Questions.ShouldContain(question => question.Id == questionCId && question.RosterVector.Length == 0);

        private static EventContext eventContext;
        private static Guid questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Interview interview;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] emptyRosterVector = new decimal[] { };
    }
}
using System;
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

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_reevaluating_whole_interview_and_questionnaire_has_mandatory_date_question_with_validation : InterviewTestsContext
    {
        Establish context = () =>
        {
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var validationExpression = "i'm validation exression";

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(dateQuestionId) == true
                && _.GetQuestionType(dateQuestionId) == QuestionType.DateTime
                && _.GetAllMandatoryQuestions() == new[] { dateQuestionId }
                && _.IsCustomValidationDefined(dateQuestionId) == true
                && _.GetAllQuestionsWithNotEmptyValidationExpressions() == new[] { dateQuestionId }
                && _.GetCustomValidationExpression(dateQuestionId) == validationExpression
            );
            
            var expressionProcessor = Mock.Of<IExpressionProcessor>(x => x.EvaluateBooleanExpression(validationExpression, Moq.It.IsAny<Func<string, object>>()) == true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepository);

            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new DateTimeQuestionAnswered(userId, dateQuestionId, new decimal[0], DateTime.Now, new DateTime(1985, 6, 3)));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_raise_only_one_AnswerDeclaredValid_event_with_QuestionId_equal_to_dateQuestionId = () =>
            eventContext.ShouldContainEvents<AnswerDeclaredValid>(count: 1);

        It should_raise_AnswerDeclaredValid_event_with_QuestionId_equal_to_dateQuestionId = () =>
            eventContext.ShouldContainEvent<AnswerDeclaredValid>(@event => @event.QuestionId == dateQuestionId);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid dateQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}
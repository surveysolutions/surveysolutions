using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using Identity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_reevaluating_whole_interview_and_questionnaire_has_recently_enabled_question_with_validation_expression : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            conditionallyInvalidQuestionId = Guid.Parse("33333333333333333333333333333333");


            var questionaire = Mock.Of<IQuestionnaire>(_ =>
                                                        _.GetAllQuestionsWithNotEmptyValidationExpressions() == new Guid[] { conditionallyInvalidQuestionId }
                                                        && _.GetAllQuestionsWithNotEmptyCustomEnablementConditions() == new Guid[] { conditionallyInvalidQuestionId }
                                                        && _.GetCustomEnablementConditionForQuestion(conditionallyInvalidQuestionId) == enablementCondition
                                                        && _.GetCustomValidationExpression(conditionallyInvalidQuestionId) == validationCondition
                                                        && _.HasQuestion(conditionallyInvalidQuestionId) == true
                                                        && _.GetQuestionType(conditionallyInvalidQuestionId) == QuestionType.Text
                                                        && _.IsCustomValidationDefined(conditionallyInvalidQuestionId) == true);

            var expressionProcessor = new Mock<IExpressionProcessor>();

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(enablementCondition, Moq.It.IsAny<Func<string, object>>()))
                .Returns(true);

            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(validationCondition, Moq.It.IsAny<Func<string, object>>()))
            .Returns(false);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepository);
            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new TextQuestionAnswered(userId, conditionallyInvalidQuestionId, new decimal[0], DateTime.Now, "answer"));
            interview.Apply(new QuestionsDisabled(new [] { new Identity(conditionallyInvalidQuestionId, new decimal[0]) }));
            interview.Apply(new AnswersDeclaredInvalid(new[] { new Identity(conditionallyInvalidQuestionId, new decimal[0]) }));
            eventContext = new EventContext();
        };


        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_notraise_AnswersDeclaredValid_event_with_GroupId_equal_to_conditionallyInvalidQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == conditionallyInvalidQuestionId));

        It should_raise_AnswersDeclaredInvalid_event_with_GroupId_equal_to_conditionallyInvalidQuestionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(question => question.Id == conditionallyInvalidQuestionId));

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static Interview interview;
        private static Guid conditionallyInvalidQuestionId;

        private static string validationCondition = "validationCondition";
        private static string enablementCondition = "enablementCondition";
    }
}

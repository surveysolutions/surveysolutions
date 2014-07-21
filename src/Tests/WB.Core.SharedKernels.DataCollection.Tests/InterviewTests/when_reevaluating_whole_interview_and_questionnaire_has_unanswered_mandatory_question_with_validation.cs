using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_reevaluating_whole_interview_and_questionnaire_has_unanswered_mandatory_question_with_validation : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            unansweredMandatoryWithValidationQuestionId = Guid.Parse("33333333333333333333333333333333");


            var questionaire = Mock.Of<IQuestionnaire>(_ =>
                                                        _.GetAllQuestionsWithNotEmptyValidationExpressions() == new Guid[] { unansweredMandatoryWithValidationQuestionId }
                                                        && _.HasQuestion(unansweredMandatoryWithValidationQuestionId) == true
                                                        && _.GetQuestionType(unansweredMandatoryWithValidationQuestionId) == QuestionType.Text
                                                        && _.IsCustomValidationDefined(unansweredMandatoryWithValidationQuestionId) == true
                                                        && _.GetAllQuestionsWithNotEmptyValidationExpressions() == new Guid[] { unansweredMandatoryWithValidationQuestionId }
                                                        && _.GetAllMandatoryQuestions() == new Guid[] { unansweredMandatoryWithValidationQuestionId });

            var expressionProcessor = new Mock<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>();

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
                .Returns(true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_not_raise_AnswersDeclaredValid_event_with_GroupId_equal_to_conditionallyInvalidQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == unansweredMandatoryWithValidationQuestionId));

        It should_raise_AnswersDeclaredInvalid_event_with_GroupId_equal_to_conditionallyInvalidQuestionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(question => question.Id == unansweredMandatoryWithValidationQuestionId));

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static Interview interview;
        private static Guid unansweredMandatoryWithValidationQuestionId;
    }
}

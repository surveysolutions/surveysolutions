using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#, KP-4391 Interview reevalution")]
    internal class when_reevaluating_whole_interview_and_questionnaire_has_mandatory_disabled_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            conditionallyDisabledMandatoryQuestionId = Guid.Parse("33333333333333333333333333333333");


            var questionaire = Mock.Of<IQuestionnaire>(_ =>
                                                       /* _.GetAllQuestionsWithNotEmptyCustomEnablementConditions() == new Guid[] { conditionallyDisabledMandatoryQuestionId }
                                                        &&*/ _.GetAllMandatoryQuestions() == new Guid[] { conditionallyDisabledMandatoryQuestionId });

            //var expressionProcessor = new Mock<IExpressionProcessor>();

            //setup expression processor throw exception
//            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
//                .Returns(false);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            //Mock.Get(ServiceLocator.Current)
            //    .Setup(locator => locator.GetInstance<IExpressionProcessor>())
            //    .Returns(expressionProcessor.Object);

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

        It should_not_raise_QuestionDisabled_event_with_GroupId_equal_to_conditionallyEnabledQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredInvalid>(@event
             => @event.Questions.Any(x => x.Id == conditionallyDisabledMandatoryQuestionId));

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static Interview interview;
        private static Guid conditionallyDisabledMandatoryQuestionId;
    }
}

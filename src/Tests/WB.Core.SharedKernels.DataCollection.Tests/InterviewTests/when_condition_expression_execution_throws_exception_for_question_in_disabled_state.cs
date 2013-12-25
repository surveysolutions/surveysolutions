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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_condition_expression_execution_throws_exception_for_question_in_disabled_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            conditionallyDisabledQuestionId = Guid.Parse("11111111111111111111111111111111");
            answeringQuestionId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(conditionallyDisabledQuestionId) == true
                                                        && _.GetQuestionType(conditionallyDisabledQuestionId) == QuestionType.Text
                                                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(conditionallyDisabledQuestionId) == new [] { new QuestionIdAndVariableName(answeringQuestionId, "var name") }

                                                        && _.HasQuestion(answeringQuestionId) == true
                                                        && _.GetQuestionType(answeringQuestionId) == QuestionType.Numeric
                                                        && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(answeringQuestionId) == new Guid[] { conditionallyDisabledQuestionId });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);

            //give an answer which would disable conditionallyDisabledQuestion at first
            interview.AnswerNumericRealQuestion(userId, answeringQuestionId, new decimal[] { }, DateTime.Now, 5);

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>())).Throws(new Exception());

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerNumericRealQuestion(userId, answeringQuestionId, new decimal[] { }, DateTime.Now, 0);

        It should_not_raise_QuestionDisabled_event_with_QuestionId_equal_to_conditionallyDisabledQuestionId = () =>
            eventContext.ShouldNotContainEvent<QuestionDisabled>(@event
              => @event.QuestionId == conditionallyDisabledQuestionId );

        It should_raise_QuestionEnabled_event_with_QuestionId_equal_to_conditionallyDisabledQuestionId = () =>
            eventContext.ShouldContainEvent<QuestionEnabled>(@event
             => @event.QuestionId == conditionallyDisabledQuestionId);

        private static EventContext eventContext;
        private static Guid conditionallyDisabledQuestionId;
        private static Interview interview;
        private static Guid userId;
        private static Guid answeringQuestionId;
    }
}

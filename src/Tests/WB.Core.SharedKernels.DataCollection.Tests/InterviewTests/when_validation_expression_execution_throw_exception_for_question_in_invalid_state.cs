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

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_validation_expression_execution_throw_exception_for_question_in_valid_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            validatingQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => _.HasQuestion(validatingQuestionId) == true
                                                        && _.GetQuestionType(validatingQuestionId) == QuestionType.Numeric
                                                        && _.GetQuestionsInvolvedInCustomValidation(validatingQuestionId) == new [] { new QuestionIdAndVariableName(validatingQuestionId,"var name") }
                                                        && _.IsCustomValidationDefined(validatingQuestionId)==true
                                                        );
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

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>())).Throws(new Exception());
            
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerNumericRealQuestion(userId, validatingQuestionId, new int[] { }, DateTime.Now, 0);

        It should_not_raise_AnswerDeclaredValid_event_with_QuestionId_equal_to_validatingQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswerDeclaredValid>(@event
              => @event.QuestionId == validatingQuestionId);

        It should_raise_AnswerDeclaredInvalid_event_with_QuestionId_equal_to_validatingQuestionId = () =>
            eventContext.ShouldContainEvent<AnswerDeclaredInvalid>(@event
             => @event.QuestionId == validatingQuestionId);

        private static EventContext eventContext;
        private static Guid validatingQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}

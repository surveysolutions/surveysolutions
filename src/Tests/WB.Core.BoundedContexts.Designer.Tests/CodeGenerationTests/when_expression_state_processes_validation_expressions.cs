using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    [Ignore("bulk test run failed on server build")]
    internal class when_expression_state_processes_validation_expressions : CodeGenerationTestsContext
    {
        private Establish context = () =>
        {
            eventContext = new EventContext();

            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            questionnaireDocument = CreateQuestionnairDocumenteWithOneNumericIntegerQuestionAndRosters(questionnaireId, questionId, rosterId);

            IInterviewExpressionStateProvider interviewExpressionStateProvider = GetInterviewExpressionStateProvider(questionnaireDocument);


            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStateProvider>())
                .Returns(interviewExpressionStateProvider);

            state = interviewExpressionStateProvider.GetExpressionState(questionnaireId, 0).Clone();

            state.UpdateIntAnswer(questionId, new decimal[0], 4);
            state.AddRoster(rosterId, new decimal[0], 1, null);
        };

        private Because of = () =>
            state.ProcessValidationExpressions(out questionsToBeValid, out questionsToBeInvalid);

        private It should_valid_question_count_equal_2 = () =>
            questionsToBeValid.Count.ShouldEqual(2);


        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111112");
        private static Guid rosterId = Guid.Parse("21111111111111111111111111111112");

        private static QuestionnaireDocument questionnaireDocument;

        private static IInterviewExpressionState state;

        private static List<Identity> questionsToBeValid;
        private static List<Identity> questionsToBeInvalid;
        
        private static EventContext eventContext;
    }
}

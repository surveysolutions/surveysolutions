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
    internal class when_expression_state_processes_condition_expressions : CodeGenerationTestsContext
    {
        private Establish context = () =>
        {
            eventContext = new EventContext();

            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            questionnaireDocument = CreateQuestionnairDocumenteWithTwoNumericIntegerQuestionAndConditionalGroup(questionnaireId, questionId);

            IInterviewExpressionStateProvider interviewExpressionStateProvider = GetInterviewExpressionStateProvider(questionnaireDocument);


            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStateProvider>())
                .Returns(interviewExpressionStateProvider);

            state = interviewExpressionStateProvider.GetExpressionState(questionnaireId, 0).Clone();

            state.UpdateIntAnswer(questionId, new decimal[0], 2);
        };

        private Because of = () =>
            state.ProcessConditionExpressions(out questionsToBeEnabled, out questionsToBeDisabled, out groupsToBeEnabled, out groupsToBeDisabled);

        private It should_valid_question_count_equal_1 = () =>
            questionsToBeDisabled.Count.ShouldEqual(1);

        private static Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111112");
        private static QuestionnaireDocument questionnaireDocument;

        private static IInterviewExpressionState state;
        private static List<Identity> questionsToBeEnabled;
        private static List<Identity> questionsToBeDisabled;
        private static List<Identity> groupsToBeEnabled;
        private static List<Identity> groupsToBeDisabled;

        private static EventContext eventContext;
    }
}

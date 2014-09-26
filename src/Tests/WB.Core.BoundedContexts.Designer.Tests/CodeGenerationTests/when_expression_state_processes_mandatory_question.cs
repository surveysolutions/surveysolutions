using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    [Ignore("bulk test run failed on server build")]
    internal class when_expression_state_processes_mandatory_question : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            questionnaireDocument = CreateQuestionnaireDocumenteHavingMandatoryQuestions(questionnaireId, question1Id, question2Id, question3Id, question4Id);

            IInterviewExpressionStatePrototypeProvider interviewExpressionStateProvider = GetInterviewExpressionStateProvider(questionnaireDocument);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns(interviewExpressionStateProvider);

            state = interviewExpressionStateProvider.GetExpressionState(questionnaireId, 0).Clone();

        };

        Because of = () =>
            state.ProcessValidationExpressions(out questionsToBeValid, out questionsToBeInvalid);

        It should_valid_question_count_equal_4 = () =>
            questionsToBeValid.Count.ShouldEqual(4);

        It should_invalid_question_count_equal_0 = () =>
            questionsToBeInvalid.Count.ShouldEqual(0);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid question1Id = Guid.Parse("11111111111111111111111111111112");
        private static Guid question2Id = Guid.Parse("21111111111111111111111111111112");
        private static Guid question3Id = Guid.Parse("31111111111111111111111111111112");
        private static Guid question4Id = Guid.Parse("41111111111111111111111111111112");

        private static QuestionnaireDocument questionnaireDocument;

        private static IInterviewExpressionState state;

        private static List<Identity> questionsToBeValid;
        private static List<Identity> questionsToBeInvalid;
    }
}

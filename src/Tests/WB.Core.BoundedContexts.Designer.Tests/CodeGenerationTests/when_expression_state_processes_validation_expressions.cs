using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    internal class when_expression_state_processes_validation_expressions : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = RemoteFunc.Invoke(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid questionId = Guid.Parse("11111111111111111111111111111112");
                Guid rosterId = Guid.Parse("21111111111111111111111111111112");

                List<Identity> questionsToBeValid;
                List<Identity> questionsToBeInvalid;

                var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
                ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumenteWithOneNumericIntegerQuestionAndRosters(questionnaireId, questionId, rosterId);
                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 4);
                state.AddRoster(rosterId, new decimal[0], 1, null);

                state.ProcessValidationExpressions(out questionsToBeValid, out questionsToBeInvalid);

                return new InvokeResults
                {
                    ValidQuestionsCount = questionsToBeValid.Count,
                    InvalidQuestionsCount = questionsToBeInvalid.Count,
                };
            });

        It should_valid_question_count_equal_2 = () =>
            results.ValidQuestionsCount.ShouldEqual(2);

        It should_invalid_question_count_equal_0 = () =>
            results.InvalidQuestionsCount.ShouldEqual(0);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public int ValidQuestionsCount { get; set; }
            public int InvalidQuestionsCount { get; set; }
        }
    }
}

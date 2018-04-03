using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_processes_validation_expressions : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid questionId = Guid.Parse("11111111111111111111111111111112");
                Guid rosterId = Guid.Parse("21111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumenteWithOneNumericIntegerQuestionAndRosters(questionnaireId, questionId, rosterId);
                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 4);
                state.AddRoster(rosterId, new decimal[0], 1, null);

                ValidityChanges validationChanges = state.ProcessValidationExpressions();

                return new InvokeResults
                {
                    ValidQuestionsCount = validationChanges.AnswersDeclaredValid.Count,
                    InvalidQuestionsCount = validationChanges.AnswersDeclaredInvalid.Count,
                };
            });

        [NUnit.Framework.Test] public void should_valid_question_count_equal_2 () =>
            results.ValidQuestionsCount.Should().Be(2);

        [NUnit.Framework.Test] public void should_invalid_question_count_equal_0 () =>
            results.InvalidQuestionsCount.Should().Be(0);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int ValidQuestionsCount { get; set; }
            public int InvalidQuestionsCount { get; set; }
        }
    }
}

using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_has_validation_containing_self : CodeGenerationTestsContext
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

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumenteWithOneNumericIntegerQuestionWithValidationUsingSelf(questionnaireId, questionId);
                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 4);
                

                ValidityChanges validationChanges = state.ProcessValidationExpressions();

                return new InvokeResults
                {
                    ValidQuestionsCount = validationChanges.AnswersDeclaredValid.Count,
                    InvalidQuestionsCount = validationChanges.AnswersDeclaredInvalid.Count,
                };
            });

        [NUnit.Framework.Test] public void should_valid_question_count_equal_2 () =>
            results.ValidQuestionsCount.Should().Be(1);

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

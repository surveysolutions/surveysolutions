using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_has_roster_with_corner_naming : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                string resultAssembly;

                AssemblyContext.SetupServiceLocator();

                var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireWithRosterAndNamedTextQuestions(namesToCheck);

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument,LatestQuestionnaireVersion(), out resultAssembly);

                return new InvokeResults
                {
                    Success = emitResult.Success
                };
            });

        [NUnit.Framework.Test] public void should_result_succeded () =>
            results.Success.Should().Be(true);
        
        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;
        private static string[] namesToCheck = new[] { "parent", "conditionExpressions" };


        [Serializable]
        public class InvokeResults
        {
            public bool Success { get; set; }
        }
    }
}

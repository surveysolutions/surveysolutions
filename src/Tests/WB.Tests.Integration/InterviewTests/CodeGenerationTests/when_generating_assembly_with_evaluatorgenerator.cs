using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_assembly_with_evaluatorgenerator : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid id = Guid.Parse("11111111111111111111111111111111");
                string resultAssembly;

                AssemblyContext.SetupServiceLocator();

                var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireForGeneration(id);

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument,CreateQuestionnaireVersion(), out resultAssembly);

                return new InvokeResults
                {
                    Success = emitResult.Success,
                    DiagnosticsCount = emitResult.Diagnostics.Count,
                    AssemblyLength = resultAssembly.Length,
                };
            });

        [NUnit.Framework.Test] public void should_result_succeded () =>
            results.Success.Should().Be(true);

        [NUnit.Framework.Test] public void should_assembly_length_greate_0 () =>
            results.AssemblyLength.Should().BeGreaterThan(0);

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
            public bool Success { get; set; }
            public int DiagnosticsCount { get; set; }
            public int AssemblyLength { get; set; }
        }
    }
}

using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    internal class when_generating_assembly_with_evaluatorgenerator : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = RemoteFunc.Invoke(appDomainContext.Domain, () =>
            {
                Guid id = Guid.Parse("11111111111111111111111111111111");
                string resultAssembly;

                var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
                ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

                IExpressionProcessorGenerator expressionProcessorGenerator = new QuestionnireExpressionProcessorGenerator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireForGeneration(id);

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, out resultAssembly);

                return new InvokeResults
                {
                    Success = emitResult.Success,
                    DiagnosticsCount = emitResult.Diagnostics.Count,
                    AssemblyLength = resultAssembly.Length,
                };
            });

        It should_result_succeded = () =>
            results.Success.ShouldEqual(true);

        It should_result_errors_count = () =>
            results.DiagnosticsCount.ShouldEqual(0);

        It should_assembly_length_greate_0 = () =>
            results.AssemblyLength.ShouldBeGreaterThan(0);

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
            public bool Success { get; set; }
            public int DiagnosticsCount { get; set; }
            public int AssemblyLength { get; set; }
        }
    }
}

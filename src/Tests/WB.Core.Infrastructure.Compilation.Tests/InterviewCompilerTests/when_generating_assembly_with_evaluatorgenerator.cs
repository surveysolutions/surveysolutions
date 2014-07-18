using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Practices.ServiceLocation;
using Moq;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Compilation.Tests.InterviewCompilerTests
{
    internal class when_generating_assembly_with_evaluatorgenerator
    {

        Establish context = () =>
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            interviewEvaluatorGenerator = new InterviewEvaluatorGenerator();
        };

        private Because of = () =>
            emitResult = interviewEvaluatorGenerator.GenerateEvaluator(new QuestionnaireDocument(), out resultAssembly);


        private It should_result_succeded = () =>
            emitResult.Success.ShouldEqual(true);

        private It should_result_errors_count = () =>
            emitResult.Diagnostics.Length.ShouldEqual(0);

        private It should_ = () =>
            resultAssembly.Length.ShouldBeGreaterThan(0);
        
        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;

        private static InterviewEvaluatorGenerator interviewEvaluatorGenerator;
    }
}

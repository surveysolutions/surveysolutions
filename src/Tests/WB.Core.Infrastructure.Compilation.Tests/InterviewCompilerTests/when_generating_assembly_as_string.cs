using System;
using Machine.Specifications;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.Infrastructure.Compilation.Tests.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string
    {

        Establish context = () =>
        {
            compiler = new InterviewCompiler();
        };

        private Because of = () =>
            emitResult = compiler.GenerateAssemblyAsString(id, InterviewCompiler.TestClass, out resultAssembly);


        private It should_result_succeded = () =>
            emitResult.Success.ShouldEqual(true);

        private It should_ = () =>
            resultAssembly.Length.ShouldBeGreaterThan(0);

        //readerMock.Verify(x => x.GetById("11111111111111111111111111111111"), Times.Once);

        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;
    }
}

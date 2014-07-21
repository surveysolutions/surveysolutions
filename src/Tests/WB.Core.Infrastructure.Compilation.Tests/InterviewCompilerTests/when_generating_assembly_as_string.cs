using System;
using Machine.Specifications;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.Infrastructure.Compilation.Tests.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string
    {

        Establish context = () =>
        {
            compiler = new RoslynInterviewCompiler();
        };

        private Because of = () =>
            emitResult = compiler.GenerateAssemblyAsString(id, testClassToCompile, new string[0], out resultAssembly);


        private It should_result_succeded = () =>
            emitResult.Success.ShouldEqual(true);

        private It should_ = () =>
            resultAssembly.Length.ShouldBeGreaterThan(0);
        
        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;

        public static string testClassToCompile =
            @"using System;
                using System.Collections.Generic;
                using System.Linq;
                using WB.Core.SharedKernels.DataCollection;
                public class InterviewEvaluator : IInterviewEvaluator
            {
                public static object Evaluate()
                {
                    return 2+2*2;
                }

                private List<int> values = new List<int>() {40, 2};

                public int Test()
                {
                    return values.Sum(i => i);
                }

                public List<Identity> CalculateValidationChanges()
                {
                    return new List<Identity>();
                }

                public List<Identity> CalculateConditionChanges()
                {
                    return new List<Identity>();
                }
 
            }";
    }
}

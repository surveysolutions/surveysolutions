using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string : InterviewCompilerTestsContext
    {
        Establish context = () =>
        {
            compiler = CreateRoslynCompiler();
            referencedPortableAssemblies = CreateReferencesForCompiler();
            generatedClasses.Add("main", testClassToCompile);
        };

        Because of = () =>
            emitResult = compiler.TryGenerateAssemblyAsStringAndEmitResult(id, generatedClasses, referencedPortableAssemblies, out resultAssembly);


        It should_result_succeded = () =>
            emitResult.Success.ShouldEqual(true);

        It should_diagnostics_count_equal_0 = () =>
            emitResult.Diagnostics.Count().ShouldEqual(0);

        It should_ = () =>
            resultAssembly.Length.ShouldBeGreaterThan(0);
        
        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;
        private static Dictionary<string, string> generatedClasses = new Dictionary<string, string>();
        private static PortableExecutableReference[] referencedPortableAssemblies;

        public static string testClassToCompile =
            @"using System.Collections.Generic;
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

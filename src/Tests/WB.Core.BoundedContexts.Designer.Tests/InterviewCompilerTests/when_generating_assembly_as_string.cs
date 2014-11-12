using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Tests.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string
    {

        Establish context = () =>
        {
            compiler = new RoslynCompiler(
                new DefaultDynamicCompillerSettings()
                {
                    PortableAssembliesPath =
                        "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.0\\Profile\\Profile24",
                    DefaultReferencedPortableAssemblies = new[] {"System.dll", "System.Core.dll", "mscorlib.dll"}
                }, new FileSystemIOAccessor());
        };

        private Because of = () =>
            emitResult = compiler.GenerateAssemblyAsString(id, testClassToCompile, new string[0], out resultAssembly);


        private It should_result_succeded = () =>
            emitResult.Success.ShouldEqual(true);

        private It should_diagnostics_count_equal_0 = () =>
            emitResult.Diagnostics.Count().ShouldEqual(0);

        private It should_ = () =>
            resultAssembly.Length.ShouldBeGreaterThan(0);
        
        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;

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

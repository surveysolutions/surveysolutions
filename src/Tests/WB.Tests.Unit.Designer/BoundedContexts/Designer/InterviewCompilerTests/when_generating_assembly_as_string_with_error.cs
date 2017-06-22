using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string_with_error : InterviewCompilerTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            compiler = CreateRoslynCompiler();
            referencedPortableAssemblies = CreateReferencesForCompiler();

            var classes = new Dictionary<string, string>();
            classes.Add("main", testClassToCompile);
            classes.Add(fileName, testClassToCompilePartTwo);

            generatedClasses = classes;
        }

        private void BecauseOf() =>
            emitResult = compiler.TryGenerateAssemblyAsStringAndEmitResult(id, generatedClasses, referencedPortableAssemblies, out resultAssembly);


        [NUnit.Framework.Test] public void should_faled () =>
            emitResult.Success.ShouldEqual(false);

        [NUnit.Framework.Test] public void should_diagnostics_count_equal_1 () =>
            emitResult.Diagnostics.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_diagnostics_FilePath_equals_fileName () =>
            emitResult.Diagnostics.First().Location.SourceTree.FilePath.ShouldEqual(fileName);

        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;
        private static Dictionary<string, string> generatedClasses;
        private static PortableExecutableReference[] referencedPortableAssemblies;


        private static string fileName = "validation:11111111111111111111111111111112";

        public static string testClassToCompile =
            @"using System.Collections.Generic;
                using System.Linq;
                using WB.Core.SharedKernels.DataCollection;
                public partial class InterviewEvaluator : IInterviewEvaluator
            {
                public static object Evaluate()
                {
                    return 2+2*2;
                }

                private List<int> values = new List<int>() {40, 2};

                public List<Identity> CalculateValidationChanges()
                {
                    return new List<Identity>();
                }

                public List<Identity> CalculateConditionChanges()
                {
                    return new List<Identity>();
                }
 
            }";

        public static string testClassToCompilePartTwo =
            @"using System.Collections.Generic;
                using System.Linq;
                using WB.Core.SharedKernels.DataCollection;
                public partial class InterviewEvaluator
            {
               public int Test()
                {
                    return values.Sum(i => i)>;
                } 
            }";
    }
}

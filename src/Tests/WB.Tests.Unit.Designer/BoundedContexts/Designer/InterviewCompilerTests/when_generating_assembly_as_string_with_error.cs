using System.Collections.Generic;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string_with_error : InterviewCompilerTestsContext
    {
        [Test]
        public void should_report_error () {
            var compiler = CreateRoslynCompiler();
            var referencedPortableAssemblies = CreateReferencesForCompiler();
            string fileName = "validation:11111111111111111111111111111112";

            var classes =
                new Dictionary<string, string>
                {
                    {
                        "main", TestClassToCompile
                    },
                    {
                        fileName, TestClassToCompilePartTwo
                    }
                };

            var generatedClasses = classes;

            var emitResult = compiler.TryGenerateAssemblyAsStringAndEmitResult(Id.g1, generatedClasses, referencedPortableAssemblies, out string _);

            Assert.That(emitResult.Success, Is.False);
            Assert.That(emitResult.Diagnostics, Has.Count.EqualTo(1));
            Assert.That(emitResult.Diagnostics[0].Location, Is.EqualTo(fileName));
        }


        const string TestClassToCompile =
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

        const string TestClassToCompilePartTwo =
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

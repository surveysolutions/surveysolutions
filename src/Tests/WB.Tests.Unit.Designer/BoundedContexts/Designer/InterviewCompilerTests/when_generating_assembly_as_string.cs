using System.Collections.Generic;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string : InterviewCompilerTestsContext
    {
        [Test] public void context ()
        {
            var compiler = CreateRoslynCompiler();
            var referencedPortableAssemblies = CreateReferencesForCompiler();
            var generatedClasses = new Dictionary<string, string> {{"main", TestClassToCompile}};

            //Act
            var emitResult = compiler.TryGenerateAssemblyAsStringAndEmitResult(Id.g1, generatedClasses,
                referencedPortableAssemblies, out string _);

            //Assert
            Assert.That(emitResult, Has.Property(nameof(emitResult.Success)).EqualTo(true));
            Assert.That(emitResult.Diagnostics, Is.Empty);
        }

        const string TestClassToCompile =
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

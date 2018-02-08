using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class InterviewCompilerTestsContext
    {
        public static RoslynCompiler CreateRoslynCompiler()
        {
            return new RoslynCompiler();
        }


        public static List<MetadataReference> CreateReferencesForCompiler()
        {
            return Create.DynamicCompilerSettingsProvider().GetAssembliesToReference();
        }
    }
}

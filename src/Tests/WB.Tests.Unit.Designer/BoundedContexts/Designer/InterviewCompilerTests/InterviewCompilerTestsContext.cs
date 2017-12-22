using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.FileSystem;

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
            var provider = new DynamicCompilerSettingsProvider(Mock.Of<ICompilerSettings>(), Mock.Of<IFileSystemAccessor>());
            return provider.GetAssembliesToReference(22);
        }
    }
}

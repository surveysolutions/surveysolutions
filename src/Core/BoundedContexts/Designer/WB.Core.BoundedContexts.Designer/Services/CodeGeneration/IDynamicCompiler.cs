using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompiler
    {
        GenerationResult TryGenerateAssemblyAsStringAndEmitResult(
            Guid templateId,
            Dictionary<string, string> generatedClasses,
            IEnumerable<MetadataReference> referencedPortableAssemblies,
            out string generatedAssembly);
    }
}

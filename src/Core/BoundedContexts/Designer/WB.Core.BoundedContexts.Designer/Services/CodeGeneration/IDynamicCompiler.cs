using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompiler
    {
        EmitResult TryGenerateAssemblyAsStringAndEmitResult(
            Guid templateId,
            Dictionary<string, string> generatedClasses,
            PortableExecutableReference[] referencedPortableAssemblies,
            IDynamicCompilerSettings settings,
            out string generatedAssembly);
    }
}

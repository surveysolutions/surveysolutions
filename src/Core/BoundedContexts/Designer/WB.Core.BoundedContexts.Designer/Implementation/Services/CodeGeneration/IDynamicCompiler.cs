using System;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public interface IDynamicCompiler
    {
        EmitResult GenerateAssemblyAsString(Guid templateId, string classCode, string[] referencedPortableAssemblies, out string generatedAssembly);
    }
}

using System;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.Infrastructure.Compilation
{
    public interface IDynamicCompiler
    {
        EmitResult GenerateAssemblyAsString(Guid templateId, string classCode, out string generatedAssembly);
    }
}

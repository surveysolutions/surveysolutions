using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompiler
    {
        EmitResult GenerateAssemblyAsString(Guid templateId, Dictionary<string, string> generatedClasses, string[] referencedPortableAssemblies, out string generatedAssembly);
    }
}

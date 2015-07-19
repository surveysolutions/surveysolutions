using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompilerSettingsProvider
    {
        IEnumerable<PortableExecutableReference> GetAssembliesToRoslyn(Version apiVersion);
    }
}
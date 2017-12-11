using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompilerSettingsProvider
    {
        IEnumerable<MetadataReference> GetAssembliesToReference(int apiVersion);
    }
}
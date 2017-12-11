using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompilerSettingsProvider
    {
        List<MetadataReference> GetAssembliesToReference(int apiVersion);
    }
}
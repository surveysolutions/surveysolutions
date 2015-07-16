using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompilerSettingsProvider
    {
        IDynamicCompilerSettings GetSettings(Version apiVersion);
    }
}
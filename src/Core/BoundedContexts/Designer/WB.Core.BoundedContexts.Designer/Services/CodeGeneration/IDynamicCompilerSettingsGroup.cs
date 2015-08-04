using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompilerSettingsGroup
    {
        IEnumerable<IDynamicCompilerSettings> SettingsCollection { get; }
    }
}
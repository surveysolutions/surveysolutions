using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface ICompilerSettings
    {
        bool EnableDump { get; }
        string DumpFolder { get; }

        IEnumerable<IDynamicCompilerSettings> SettingsCollection { get; }
    }
}
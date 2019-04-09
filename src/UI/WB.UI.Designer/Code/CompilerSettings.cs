using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.UI.Designer.Code
{
    public class CompilerSettings: ICompilerSettings
    {
        public bool EnableDump { get; set; }

        public string DumpFolder { get; set; }
    }
}

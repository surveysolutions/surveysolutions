using System.Configuration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;


namespace WB.UI.Designer
{
    public class DynamicCompilerSettingsGroup : ConfigurationSection, ICompilerSettings
    {
        [ConfigurationProperty("enableDump", IsRequired = true)]
        public bool EnableDump => (bool)this["enableDump"];

        [ConfigurationProperty("dumpFolder", IsRequired = true)]
        public string DumpFolder => (string)this["dumpFolder"];
    }
}

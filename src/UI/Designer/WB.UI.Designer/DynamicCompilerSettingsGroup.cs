using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;


namespace WB.UI.Designer
{
    public class DynamicCompilerSettingsGroup : ConfigurationSection, ICompilerSettings
    {
        [ConfigurationProperty("enableDump", IsRequired = true)]
        public bool EnableDump => (bool) this["enableDump"];

        [ConfigurationProperty("dumpFolder", IsRequired = true)]
        public string DumpFolder => (string) this["dumpFolder"];

        public IEnumerable<IDynamicCompilerSettings> SettingsCollection
        {
            get
            {
                var list = from DynamicCompilerSettings settings in settingsCollection select settings;
                return list;
            }
        }

        [ConfigurationProperty("settings")]
        DynamicCompilerSettingsCollection settingsCollection => (DynamicCompilerSettingsCollection)base["settings"];
    }

    [ConfigurationCollection(typeof(DynamicCompilerSettings), AddItemName = "dynamicCompilerSettings")]
    public class DynamicCompilerSettingsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DynamicCompilerSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DynamicCompilerSettings)element).name;
        }
    }

    public class DynamicCompilerSettings : ConfigurationElement, IDynamicCompilerSettings
    {
        public string Name => name;

        [ConfigurationProperty("name", IsRequired = true)]
        public string name => (string)this["name"];
    }
}

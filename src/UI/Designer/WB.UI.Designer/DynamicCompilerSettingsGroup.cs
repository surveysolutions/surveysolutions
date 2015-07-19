using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;


namespace WB.UI.Designer
{
    public class DynamicCompilerSettingsGroup : ConfigurationSection, IDynamicCompilerSettingsGroup
    {
        public IEnumerable<IDynamicCompilerSettings> SettingsCollection
        {
            get
            {
                var list = from DynamicCompilerSettings settings in settingsCollection select settings;
                return list;
            }
        }

        [ConfigurationProperty("settings")]
        DynamicCompilerSettingsCollection settingsCollection
        {
            get { return (DynamicCompilerSettingsCollection)base["settings"]; }
        }
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
        public string Name { get { return name; } }

        public string PortableAssembliesPath
        {
            get { return portableAssembliesPath; }
        }

        public IEnumerable<string> DefaultReferencedPortableAssemblies
        {
            get
            {
                string[] assemblies = this.portableAssemblies.Split(';').Select(s => s.Trim()).ToArray();
                return assemblies;
            }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("portableAssembliesPath", IsRequired = true)]
        public string portableAssembliesPath
        {
            get { return (string)this["portableAssembliesPath"]; }
        }

        [ConfigurationProperty("portableAssemblies")]
        public string portableAssemblies
        {
            get { return (string)base["portableAssemblies"]; }
        }
    }
}

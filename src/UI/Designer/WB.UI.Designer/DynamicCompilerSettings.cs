using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.UI.Designer
{
    public class DynamicCompilerSettings : ConfigurationSection, IDynamicCompilerSettings
    {
        public string PortableAssembliesPath
        {
            get { return portableAssembliesPath; }
        }

        public IEnumerable<string> DefaultReferencedPortableAssemblies
        {
            get
            {
                return from ConfigRoslynPortableAssemblyElement assembly in portableAssemblies
                    select assembly.Name;
            }
        }

        [ConfigurationProperty("portableAssembliesPath")]
        public string portableAssembliesPath
        {
            get { return (string)this["portableAssembliesPath"]; }
        }

        [ConfigurationProperty("portableAssemblies")]
        public ConfigRoslynPortableAssemblyCollection portableAssemblies
        {
            get { return (ConfigRoslynPortableAssemblyCollection)base["portableAssemblies"]; }
        }

        [ConfigurationCollection(typeof(ConfigRoslynPortableAssemblyElement), AddItemName = "assembly")]
        public class ConfigRoslynPortableAssemblyCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ConfigRoslynPortableAssemblyElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ConfigRoslynPortableAssemblyElement)element).Name;
            }
        }

        public class ConfigRoslynPortableAssemblyElement : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
            public string Name
            {
                get { return (string)this["name"]; }
            }
        }
    }

    
}

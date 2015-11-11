using System;
using System.Configuration;
using System.Linq;

namespace WB.UI.Shared.Web.Configuration
{
    public class CatchExceptionsByDenormalizersConfigSection : ConfigurationSection
    {
        private const string DenormalizersPropertyName = "Denormalizers";

        [ConfigurationProperty(DenormalizersPropertyName)]
        public Denormalizers Denormalizers => ((Denormalizers) (base[DenormalizersPropertyName]));

        public Type[] GetTypesOfDenormalizers()
        {
            return (from Denormalizer denormalizer in this.Denormalizers select Type.GetType(denormalizer.DenormalizerType)).ToArray();
        }
    }

    [ConfigurationCollection(typeof (Denormalizer))]
    public class Denormalizers : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Denormalizer();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Denormalizer) (element)).DenormalizerType;
        }

        public Denormalizer this[int idx] => (Denormalizer) this.BaseGet(idx);
        public new Denormalizer this[string key] => (Denormalizer) this.BaseGet(key);
    }

    public class Denormalizer: ConfigurationElement
    {
        private const string TYPE = "type";

        [ConfigurationProperty(TYPE, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string DenormalizerType
        {
            get { return ((string)(base[TYPE])); }
            set { base[TYPE] = value; }
        }
    }
}

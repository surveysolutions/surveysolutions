using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.UI.Shared.Web.Configuration
{
    public class IgnoredDenormalizersConfigSection : ConfigurationSection
    {
        private const string IgnoredDenormalizers = "IgnoredDenormalizers";

        [ConfigurationProperty(IgnoredDenormalizers)]
        public IgnoredDenormalizers IgnoredDenormalizerItems
        {
            get { return ((IgnoredDenormalizers) (base[IgnoredDenormalizers])); }
        }

        public Type[] GetIgnoredTypes()
        {
            var result = new List<Type>();
            foreach (IgnoredDenormalizer ignoredDenormalizer in IgnoredDenormalizerItems)
            {
                result.Add(Type.GetType(ignoredDenormalizer.DenormalizerType));
            }
            return result.ToArray();
        }
    }

    [ConfigurationCollection(typeof (IgnoredDenormalizer))]
    public class IgnoredDenormalizers : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new IgnoredDenormalizer();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IgnoredDenormalizer) (element)).DenormalizerType;
        }


        public IgnoredDenormalizer this[int idx]
        {
            get { return (IgnoredDenormalizer) BaseGet(idx); }
        }

        public new IgnoredDenormalizer this[string key]
        {
            get { return (IgnoredDenormalizer) BaseGet(key); }
        }
    }

    public class IgnoredDenormalizer
        : ConfigurationElement
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

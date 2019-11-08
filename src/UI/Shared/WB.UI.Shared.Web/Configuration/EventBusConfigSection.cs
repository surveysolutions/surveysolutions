using System;
using System.Configuration;
using System.Linq;
using System.Xml;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;

namespace WB.UI.Shared.Web.Configuration
{
    public class EventBusConfigSection : ConfigurationSection
    {
        private const string EVENTHANDLERSTAGNAME = "eventHandlers";
        private const string IGNOREDAGGREGATEROOTS = "ignoredAggregateRoots";
        private const string AGGREGATEROOTID = "aggregateRootId";

        [ConfigurationProperty(EVENTHANDLERSTAGNAME)]
        public EventHandlersConfigurationElement EventHandlers
        {
            get { return ((EventHandlersConfigurationElement)(base[EVENTHANDLERSTAGNAME])); }
            set { base[EVENTHANDLERSTAGNAME] = value; }
        }

        [ConfigurationProperty(IGNOREDAGGREGATEROOTS, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(IgnoredAggregateRootConfigurationElement), AddItemName = AGGREGATEROOTID)]
        public IgnoredAggregateRootsConfigurationCollection IgnoredAggregateRoots => (IgnoredAggregateRootsConfigurationCollection)base[IGNOREDAGGREGATEROOTS];

        public EventBusSettings GetSettings()
        {
            return new EventBusSettings
            {
                DisabledEventHandlerTypes =
                    this.EventHandlers.Disabled.OfType<EventHandlerConfigurationElement>()
                        .Select(x => Type.GetType(x.EventHandlerType))
                        .ToArray(),

                EventHandlerTypesWithIgnoredExceptions =
                    this.EventHandlers.WithIgnoredExceptions.OfType<EventHandlerConfigurationElement>()
                        .Select(x => Type.GetType(x.EventHandlerType))
                        .ToArray(),
                IgnoredAggregateRoots =
                    this.IgnoredAggregateRoots.OfType<IgnoredAggregateRootConfigurationElement>()
                        .Select(x => x.Value)
                        .ToList()
            };
        }
    }

    public class EventHandlersConfigurationElement : ConfigurationElement
    {
        private const string DISABLEDTAGNAME = "disabled";
        private const string IGNOREEXCEPTIONSTAGNAME = "withIgnoredExceptions";

        [ConfigurationProperty(DISABLEDTAGNAME, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventHandlersConfigurationCollection))]
        public EventHandlersConfigurationCollection Disabled
            => (EventHandlersConfigurationCollection)base[DISABLEDTAGNAME];

        [ConfigurationProperty(IGNOREEXCEPTIONSTAGNAME, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventHandlersConfigurationCollection))]
        public EventHandlersConfigurationCollection WithIgnoredExceptions
            => (EventHandlersConfigurationCollection)base[IGNOREEXCEPTIONSTAGNAME];
    }

    [ConfigurationCollection(typeof (EventHandlerConfigurationElement))]
    public class EventHandlersConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EventHandlerConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventHandlerConfigurationElement) (element)).EventHandlerType;
        }

        public EventHandlerConfigurationElement this[int idx]
            => (EventHandlerConfigurationElement) this.BaseGet(idx);

        public new EventHandlerConfigurationElement this[string key]
            => (EventHandlerConfigurationElement) this.BaseGet(key);
    }

    public class EventHandlerConfigurationElement : ConfigurationElement
    {
        private const string TYPEATTRIBUTENAME = "type";

        [ConfigurationProperty(TYPEATTRIBUTENAME, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string EventHandlerType
        {
            get { return ((string) (base[TYPEATTRIBUTENAME])); }
            set { base[TYPEATTRIBUTENAME] = value; }
        }
    }

    public class IgnoredAggregateRootsConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new IgnoredAggregateRootConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IgnoredAggregateRootConfigurationElement)element).Value;
        }
    }

    public class IgnoredAggregateRootConfigurationElement : ConfigurationElement
    {
        private string _value;

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            _value = (string)reader.ReadElementContentAs(typeof(string), null);
        }

        public string Value => this._value;
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Web.Configuration
{
    public class EventBusConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("IgnoredAggregateRoots", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventBusConfigurationElement), AddItemName = "AggregateRootId")]
        public EventBusConfigurationCollection IgnoredAggregateRoots => (EventBusConfigurationCollection) base["IgnoredAggregateRoots"];

        [ConfigurationProperty("DisabledEventHandlers", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof (EventBusConfigurationElement), AddItemName = "DisabledEventHandler")]
        public EventBusConfigurationCollection DisabledEventHandlers => (EventBusConfigurationCollection) base["DisabledEventHandlers"];

        [ConfigurationProperty("EventHandlersWhichExceptionsShouldBeIgnored", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof (EventBusConfigurationElement), AddItemName = "EventHandler")]
        public EventBusConfigurationCollection EventHandlersWhichExceptionsShouldBeIgnored => (EventBusConfigurationCollection) base["EventHandlersWhichExceptionsShouldBeIgnored"];

        public HashSet<string> GetIgnoredAggregateRoots()
        {
            return this.IgnoredAggregateRoots.OfType<EventBusConfigurationElement>().Select(a => a.Value).ToHashSet();
        }

        public Type[] GetDisabledEventHandlers()
        {
            return
                this.DisabledEventHandlers.OfType<EventBusConfigurationElement>()
                    .Select(a => Type.GetType(a.Type))
                    .ToArray();
        }

        public Type[] GetEventHandlersWhichExceptionsShouldBeIgnored()
        {
            return this.EventHandlersWhichExceptionsShouldBeIgnored.OfType<EventBusConfigurationElement>().Select(a => Type.GetType(a.Type)).ToArray();
        }
    }
    
    public class EventBusConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EventBusConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var eventBusConfigurationElement = (EventBusConfigurationElement) element;
            if (string.IsNullOrEmpty(eventBusConfigurationElement.Value))
                return eventBusConfigurationElement.Type;
            return eventBusConfigurationElement.Value;
        }
    }

    public class EventBusConfigurationElement : ConfigurationElement
    {
        private string _value;

        protected override void DeserializeElement(XmlReader reader,bool serializeCollectionKey)
        {
            _value = (string)reader.ReadElementContentAs(typeof(string), null);
        }

        public string Value => this._value;

        [ConfigurationProperty("type", IsRequired = false)]
        public string Type => (string)base["type"];
    }
}
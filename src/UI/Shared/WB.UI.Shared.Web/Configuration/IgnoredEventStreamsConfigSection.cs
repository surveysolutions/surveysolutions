using System;
using System.Collections.Generic;
using System.Configuration;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Web.Configuration
{
    public class IgnoredEventStreamsConfigSection : ConfigurationSection
    {
        private const string IgnoredEventStreams = "IgnoredEventStreams";

        [ConfigurationProperty(IgnoredEventStreams)]
        public IgnoredEventStreams IgnoredEventStreamItems
        {
            get { return ((IgnoredEventStreams)(base[IgnoredEventStreams])); }
        }

        public HashSet<string> GetIgnoredEventStreams()
        {
            var result = new List<string>();
            foreach (IgnoredEventStream ignoredEventStream in this.IgnoredEventStreamItems)
            {
                result.Add(ignoredEventStream.EventStreamId);
            }
            return result.ToHashSet();
        }
    }

    [ConfigurationCollection(typeof(IgnoredEventStream))]
    public class IgnoredEventStreams : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new IgnoredEventStream();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IgnoredEventStream)(element)).EventStreamId;
        }


        public IgnoredEventStream this[int idx]
        {
            get { return (IgnoredEventStream)BaseGet(idx); }
        }

        public new IgnoredEventStream this[string key]
        {
            get { return (IgnoredEventStream)BaseGet(key); }
        }
    }

    public class IgnoredEventStream
       : ConfigurationElement
    {
        private const string TYPE = "eventStreamId";

        [ConfigurationProperty(TYPE, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string EventStreamId
        {
            get { return ((string)(base[TYPE])); }
            set { base[TYPE] = value; }
        }
    }
}
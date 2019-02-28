using System;
using System.Collections.Generic;
using System.Configuration;

namespace WB.UI.Shared.Web.Slack
{
    [ConfigurationCollection(typeof(ChannelsCollection), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class ChannelsCollection : ConfigurationElementCollection, IEnumerable<SlackChannelConfiguration>
    {
        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;

        protected override ConfigurationElement CreateNewElement()
        {
            return new SlackChannelConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SlackChannelConfiguration)element).Type;
        }

        public SlackChannelConfiguration this[int index]
        {
            get => (SlackChannelConfiguration)base.BaseGet(index);
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }

                base.BaseAdd(index, value);
            }
        }

        public new SlackChannelConfiguration this[String name]
        {
            get
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("Indexer 'name' cannot be null or empty.");
                }

                foreach (SlackChannelConfiguration element in this)
                {
                    if (element.Type.ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return element;
                    }
                }

                throw new InvalidOperationException("Indexer 'name' specified cannot be found in the collection.");
            }
        }

        IEnumerator<SlackChannelConfiguration> IEnumerable<SlackChannelConfiguration>.GetEnumerator()
        {
            foreach (var channel in this)
            {
                yield return channel as SlackChannelConfiguration;
            }
        }
    }
}

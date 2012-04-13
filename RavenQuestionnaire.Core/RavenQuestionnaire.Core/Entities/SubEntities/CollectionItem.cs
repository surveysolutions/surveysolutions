using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class CollectionItem 
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public CollectionItem()
        {
        }

        public CollectionItem(string _key, string _value)
        {
            this.Key = _key;
            this.Value = _value;
        }
    }
}

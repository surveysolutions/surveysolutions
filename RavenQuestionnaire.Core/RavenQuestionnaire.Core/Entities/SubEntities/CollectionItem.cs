using System;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface ICollectionItem
    {
        Guid PublicKey { get; set; }
        string Key { get; set; }
        string Value { get; set; }
    }

    public class CollectionItem :ICollectionItem
    {
        public Guid PublicKey { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public CollectionItem()
        {
        }

        public CollectionItem(Guid _id, string _key, string _value)
        {
            this.PublicKey = _id;
            this.Key = _key;
            this.Value = _value;
        }
    }
}

using System;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemView
    {
        public int Index { get; set; }

        public Guid PublicKey { get; set; }

        public string CollectionId
        {
            get { return _collectionId; }
            set { _collectionId = value; }
        }

        public string Key { get; set; }

        public string Value { get; set; }

        private string _collectionId;

        public CollectionItemView()
        {
        }

        public CollectionItemView(string collectionId)
            : this()
        {
            this.CollectionId = collectionId;
        }

        public CollectionItemView(string collectionId, ICollectionItem doc)
            : this(collectionId)
        {
            this.PublicKey = doc.PublicKey;
            this.Key = doc.Key;
            this.Value = doc.Value;
        }
    }
}

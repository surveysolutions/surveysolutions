using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities
{
    public interface ICollection
    {
        string Name { get; set; }
        string Id { get; set; }
    }

    public class Collection : IEntity<CollectionDocument>
    {
        private CollectionDocument innerDocument;

        public string CollectionId { get { return innerDocument.Id; } }

        CollectionDocument IEntity<CollectionDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }
        
        public Collection(CollectionDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        public Collection(string title, List<CollectionItem> items)
        {
            innerDocument=new CollectionDocument(){ Name = title, Items = items};
        }

        public void UpdateText(string text)
        {
            innerDocument.Name = text;
        }

        public void ClearItems()
        {
            innerDocument.Items.Clear();
        }
      
        public void AddCollectionItems(List<CollectionItem> item)
        {
            foreach (var collectionItem in item)
                innerDocument.Items.Add(collectionItem);
        }

        public void DeleteItemFromCollection(Guid itemId)
        {
            var item = innerDocument.Items.Find(t => t.PublicKey == itemId);
            innerDocument.Items.Remove(item);
        }
    }
}






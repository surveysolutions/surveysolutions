using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface ICollectionDocument
    {
        string Id { get; set; }

        string Name { get; set; }
    }

    public interface ICollectionDocument<TCollectionItem> : ICollectionDocument
        where TCollectionItem : ICollectionItem
    {
        List<CollectionItem> Items { get; set; }
    }

    public class CollectionDocument:ICollectionDocument<ICollectionItem>
    {
        public CollectionDocument()
        {
            this.Items = new List<CollectionItem>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<CollectionItem> Items { get; set; }
        
        public void Add(IComposite c)
        {
            throw new NotImplementedException();
        }

        public void Remove(IComposite c)
        {
            //var collectionItem = this.Items.FirstOrDefault(g => c is ICollectionItem && g.Key.Equals(((ICollectionItem)c).Key));
            //if (collectionItem != null)
            //{
            //    this.Items.Remove(collectionItem);
            //    return;
            //}
        }
    }
}

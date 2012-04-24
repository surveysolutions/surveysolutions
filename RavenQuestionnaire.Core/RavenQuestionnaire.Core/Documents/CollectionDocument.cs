using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface ICollectionDocument
    {
        string Id { get; set; }

        string Name { get; set; }
    }

    public class CollectionDocument:ICollectionDocument
    {
        public CollectionDocument()
        {
            this.Items = new List<CollectionItem>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<CollectionItem> Items { get; set; }
   }
}

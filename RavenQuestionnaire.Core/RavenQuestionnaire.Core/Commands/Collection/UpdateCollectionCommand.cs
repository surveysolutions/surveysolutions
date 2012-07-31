using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Collection
{
    public class UpdateCollectionCommand:ICommand
    {
        public string CollectionId { get; set; }

        public string Name { get; private set; }

        public List<CollectionItem> Items { get; set; }

        public UserLight Executor { get; set; }

        public UpdateCollectionCommand(string collectionId, string name, List<CollectionItem> items)
        {
            this.CollectionId = collectionId;
            this.Name = name;
            this.Items = items;
        }
    }
}
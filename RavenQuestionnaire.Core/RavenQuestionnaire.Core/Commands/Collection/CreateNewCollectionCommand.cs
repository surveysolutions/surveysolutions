using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Collection
{
    public class CreateNewCollectionCommand: ICommand
    {
        public CreateNewCollectionCommand(string name, List<CollectionItem> items)
        {
            this.Name = name;
            this.Items = items;
        }

        public string Name { get; private set; }
        public List<CollectionItem> Items { get; set; }

        public UserLight Executor
        {
            get; set;
        }
    }
}

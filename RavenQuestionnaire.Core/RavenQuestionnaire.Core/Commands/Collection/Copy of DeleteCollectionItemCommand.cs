using System;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Collection
{
    public class DeleteCollectionItemCommand1 : ICommand
    {
        public string CollectionId { get; set; }

        public Guid ItemId { get; set; }

        public UserLight Executor { get; set; }

        public DeleteCollectionItemCommand1(string collectionId, UserLight executor, Guid itemId)
        {
            this.CollectionId = collectionId;
            this.Executor = executor;
            this.ItemId = itemId;
        }
    }
}

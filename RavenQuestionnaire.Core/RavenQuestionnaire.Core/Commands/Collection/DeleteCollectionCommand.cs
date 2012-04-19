using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Collection
{
    public class DeleteCollectionCommand:ICommand
    {
        public string CollectionId { get; set; }
        public UserLight Executor { get; set; }

        public DeleteCollectionCommand(string collectionId, UserLight executor)
        {
            this.CollectionId = collectionId;
            this.Executor = executor;
        }
    }
}

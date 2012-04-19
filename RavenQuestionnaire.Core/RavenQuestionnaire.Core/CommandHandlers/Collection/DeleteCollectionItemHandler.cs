using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Collection;

namespace RavenQuestionnaire.Core.CommandHandlers.Collection
{
    public class DeleteCollectionItemHandler:ICommandHandler<DeleteCollectionItemCommand>
    {
        private readonly ICollectionRepository _collectionRepository;

        public DeleteCollectionItemHandler(ICollectionRepository collectionRepository)
        {
            _collectionRepository = collectionRepository;
        }

        public void Handle(DeleteCollectionItemCommand command)
        {
            var entity = _collectionRepository.Load(IdUtil.CreateCollectionId(command.CollectionId));
            entity.DeleteItemFromCollection(command.ItemId);
        }
    }
}
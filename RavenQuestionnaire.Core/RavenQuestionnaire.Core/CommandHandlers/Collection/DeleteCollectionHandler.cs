using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Collection;

namespace RavenQuestionnaire.Core.CommandHandlers.Collection
{
    public class DeleteCollectionHandler:ICommandHandler<DeleteCollectionCommand>
    {
        private readonly ICollectionRepository _collectionRepository;

        public DeleteCollectionHandler(ICollectionRepository collectionRepository)
        {
            _collectionRepository = collectionRepository;
        }

        public void Handle(DeleteCollectionCommand command)
        {
            var entity = _collectionRepository.Load(IdUtil.CreateCollectionId(command.CollectionId));
            this._collectionRepository.Remove(entity);
        }
    }
}
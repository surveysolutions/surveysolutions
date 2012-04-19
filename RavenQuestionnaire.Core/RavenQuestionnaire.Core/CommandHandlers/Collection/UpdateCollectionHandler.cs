using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Collection
{
    public class UpdateCollectionHandler:ICommandHandler<UpdateCollectionCommand>
    {
        private readonly ICollectionRepository _collectionRepository;

        public UpdateCollectionHandler(ICollectionRepository collectionRepository)
        {
            _collectionRepository = collectionRepository;
        }

        public void Handle(UpdateCollectionCommand command)
        {
            var entity = _collectionRepository.Load(IdUtil.CreateCollectionId(command.CollectionId));
            entity.UpdateText(command.Name);
            entity.ClearItems();
            entity.AddCollectionItem(command.Items);
        }
    }
}

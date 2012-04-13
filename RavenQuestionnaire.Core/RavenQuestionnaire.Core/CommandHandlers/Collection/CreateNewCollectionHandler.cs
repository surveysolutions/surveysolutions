using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Collection;

namespace RavenQuestionnaire.Core.CommandHandlers.Collection
{
    public class CreateNewCollectionHandler:ICommandHandler<CreateNewCollectionCommand>
    {
        private ICollectionRepository _repository;

        public CreateNewCollectionHandler(ICollectionRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(CreateNewCollectionCommand command)
        {
            Entities.Collection entity = new Entities.Collection(command.Name, command.Items);
            this._repository.Add(entity);
        }
    }
}

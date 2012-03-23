using RavenQuestionnaire.Core.Commands.Location;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Location
{
    public class CreateNewLocationHandler : ICommandHandler<CreateNewLocationCommand>
    {
        private ILocationRepository _repository;
        public CreateNewLocationHandler(ILocationRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(CreateNewLocationCommand command)
        {
            Entities.Location entity= new Entities.Location(command.Location);
            this._repository.Add(entity);
        }
    }
}

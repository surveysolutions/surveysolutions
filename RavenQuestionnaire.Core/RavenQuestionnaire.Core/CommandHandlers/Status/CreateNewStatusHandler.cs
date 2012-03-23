using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Status
{
    public class CreateNewStatusHandler: ICommandHandler<CreateNewStatusCommand>
    {
        private IStatusRepository _repository;

        public CreateNewStatusHandler(IStatusRepository repository)
        {
            this._repository = repository;
            //this._locationalRepository = locationRepository;
        }

        public void Handle(CreateNewStatusCommand command)
        {
            Entities.Status newStatus = new Entities.Status(command.Title, command.IsInitial, command.QuestionnaireId);
            _repository.Add(newStatus);
        }
    }
}

using System;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
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
            Status newStatus = new Status(command.Title);
            _repository.Add(newStatus);
        }
    }
}

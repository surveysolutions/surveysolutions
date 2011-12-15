using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
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
            Location entity= new Location(command.Location);
            this._repository.Add(entity);
        }
    }
}

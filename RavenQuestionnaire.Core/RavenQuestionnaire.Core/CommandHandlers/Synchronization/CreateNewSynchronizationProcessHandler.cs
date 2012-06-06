using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Synchronization
{
    [CommandHandler(IgnoreAsEvent = true)]
    public class CreateNewSynchronizationProcessHandler : ICommandHandler<CreateNewSynchronizationProcessCommand>
    {
        private ISyncProcessRepository repository;
        public CreateNewSynchronizationProcessHandler(ISyncProcessRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(CreateNewSynchronizationProcessCommand command)
        {
            var process = new SyncProcess(command.ProcessGuid);
            this.repository.Add(process);
        }
    }
}

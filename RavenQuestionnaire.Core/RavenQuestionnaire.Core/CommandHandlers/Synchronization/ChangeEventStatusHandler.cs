using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Synchronization
{
    [CommandHandler(IgnoreAsEvent = true)]
    public class ChangeEventStatusHandler : ICommandHandler<ChangeEventStatusCommand>
    {
        private ISyncProcessRepository repository;
        public ChangeEventStatusHandler(ISyncProcessRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(ChangeEventStatusCommand command)
        {
            var process = this.repository.Load(command.ProcessGuid.ToString());
            process.SetEventState(command.EventGuid, command.Status);
        }
    }
}

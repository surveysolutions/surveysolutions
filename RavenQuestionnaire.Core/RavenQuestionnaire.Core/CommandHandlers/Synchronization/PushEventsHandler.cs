using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Synchronization
{
    public class PushEventsHandler : ICommandHandler<PushEventsCommand>
    {
         private ISyncProcessRepository repository;
         public PushEventsHandler(ISyncProcessRepository repository)
        {
            this.repository = repository;
        }

         public void Handle(PushEventsCommand command)
        {
            var process = this.repository.Load(command.ProcessGuid.ToString());
             process.AddEvents(command.Events);
        }
    }
}

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
    public class EndProcessHandler : ICommandHandler<EndProcessComand>
    {
         private ISyncProcessRepository repository;
         public EndProcessHandler(ISyncProcessRepository repository)
        {
            this.repository = repository;
        }

         public void Handle(EndProcessComand command)
        {
            var process = this.repository.Load(command.ProcessGuid.ToString());
           process.EndProcess();
             // this.repository.Remove(process);
        }
    }
}

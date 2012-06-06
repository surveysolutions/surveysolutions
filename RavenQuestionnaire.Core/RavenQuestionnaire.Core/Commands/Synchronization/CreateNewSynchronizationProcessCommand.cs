using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    public class CreateNewSynchronizationProcessCommand:ICommand
    {
        public UserLight Executor { get; set; }
        public Guid ProcessGuid { get; set; }
        public  CreateNewSynchronizationProcessCommand(Guid processGuid,UserLight executor)
        {
            this.Executor = executor;
            this.ProcessGuid = processGuid;
        }
    }
}

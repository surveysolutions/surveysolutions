using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    public class ChangeEventStatusCommand:ICommand
    {
        public UserLight Executor { get; set; }
        public EventState Status { get; set; }
        public Guid EventGuid { get; set; }
        public Guid ProcessGuid { get; set; }
        public ChangeEventStatusCommand(Guid processGuid,Guid eventGuid, EventState status, UserLight executor)
        {
            this.Executor = executor;
            this.EventGuid = eventGuid;
            this.Status = status;
            this.ProcessGuid = processGuid;
        }
    }
}

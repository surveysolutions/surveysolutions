using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    public class PushEventsCommand:ICommand
    {
        public UserLight Executor { get; set; }
        public IEnumerable<EventDocument> Events { get; set; }
        public Guid ProcessGuid { get; set; }
        public PushEventsCommand(Guid processGuid, IEnumerable<EventDocument> events, UserLight executor)
        {
            this.Executor = executor;
            this.Events = events;
            this.ProcessGuid = processGuid;
        }
    }
}

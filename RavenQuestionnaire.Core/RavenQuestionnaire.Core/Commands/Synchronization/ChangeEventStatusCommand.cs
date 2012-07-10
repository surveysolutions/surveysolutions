using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "ChangeAggregateRootStatus")]
    public class ChangeEventStatusCommand : CommandBase
    {
        public EventState Status { get; set; }
        public Guid AggregateRootPublicKey { get; set; }
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }

        public ChangeEventStatusCommand(Guid processGuid, Guid aggregateRootPublicKey, EventState status)
        {
            this.AggregateRootPublicKey = aggregateRootPublicKey;
            this.Status = status;
            this.ProcessGuid = processGuid;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "EndProcess")]
    public class EndProcessComand : CommandBase
    {
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }
        public EndProcessComand(Guid processGuid)
        {
            this.ProcessGuid = processGuid;
        }
    }
}

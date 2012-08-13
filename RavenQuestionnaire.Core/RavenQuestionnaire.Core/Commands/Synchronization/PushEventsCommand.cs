using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "PushAggregateRootEventStream")]
    public class PushEventsCommand : CommandBase
    {
        public IEnumerable<ProcessedEventChunk> AggregateRoots { get; set; }
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }
        public PushEventsCommand(Guid processGuid, IEnumerable<ProcessedEventChunk> aggregateRoots)
        {
            this.AggregateRoots = aggregateRoots;
            this.ProcessGuid = processGuid;
        }
        public PushEventsCommand(Guid processGuid, IEnumerable<AggregateRootEvent> aggregateRoots)
        {
            this.AggregateRoots = new[] {new ProcessedEventChunk(aggregateRoots)};
            this.ProcessGuid = processGuid;
        }
    }
}

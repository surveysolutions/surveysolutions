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
        public IList<ProcessedEventChunk> EventChuncks { get; set; }
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }
        public PushEventsCommand(Guid processGuid, IList<ProcessedEventChunk> eventChuncks)
        {
            this.EventChuncks = eventChuncks;
            this.ProcessGuid = processGuid;
        }
        public PushEventsCommand(Guid processGuid, IList<IEnumerable<AggregateRootEvent>> eventChuncks)
        {
           // this.AggregateRoots = new[] {new ProcessedEventChunk(aggregateRoots)};
            var result = new List<ProcessedEventChunk>(eventChuncks.Count);
            result.AddRange(eventChuncks.Select(aggregateRootEvents => new ProcessedEventChunk(aggregateRootEvents)));
            this.EventChuncks = result;
            this.ProcessGuid = processGuid;
        }
    }
}

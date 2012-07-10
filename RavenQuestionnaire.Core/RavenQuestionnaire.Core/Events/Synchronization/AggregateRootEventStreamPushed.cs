using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Events.Synchronization
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AggregateRootEventStreamPushed")]
    public class AggregateRootEventStreamPushed
    {
        public IEnumerable<ProcessedAggregateRoot> AggregateRoots { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Events.Synchronization
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AggregateRootStatusChanged")]
    public class AggregateRootStatusChanged
    {
        public EventState Status { get; set; }
        public Guid EventChunckPublicKey { get; set; }
    }
}

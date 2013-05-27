using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class StoredEventWithoutPayload
    {
        public Guid EventSourceId { get; set; }

        public Guid EventIdentifier { get; set; }
        public long EventSequence { get; set; }
    }
}

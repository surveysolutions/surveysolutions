using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.DenormalizerStorage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.Infrastructure;

namespace Ncqrs.Restoring.EventStapshoot
{
    public class SnapshotDenormalizer:IEventHandler<SnapshootLoaded>
    {
        private readonly IDenormalizerStorage<CommittedEvent> store;

        public SnapshotDenormalizer(IDenormalizerStorage<CommittedEvent> store)
        {
            this.store = store;
        }

        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            this.store.Store(
                new CommittedEvent(evnt.CommitId, evnt.EventIdentifier, evnt.EventSourceId, evnt.EventSequence,
                                   evnt.EventTimeStamp, evnt.Payload, evnt.EventVersion), evnt.EventSourceId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.Services
{
    class AggregateRootPrototypePromoterService : IAggregateRootPrototypePromoterService
    {
        private readonly ILiteEventBus liteEventBus;
        private readonly IInMemoryEventStore inMemoryEventStore;
        private readonly IEventStore eventStore;
        private readonly IAggregateRootPrototypeService prototypeService;
        private readonly IAggregateRootCache cache;

        public AggregateRootPrototypePromoterService(ILiteEventBus liteEventBus,
            IInMemoryEventStore inMemoryEventStore, IEventStore eventStore, 
            IAggregateRootPrototypeService prototypeService, IAggregateRootCache cache)
        {
            this.liteEventBus = liteEventBus;
            this.inMemoryEventStore = inMemoryEventStore;
            this.eventStore = eventStore;
            this.prototypeService = prototypeService;
            this.cache = cache;
        }

        public void MaterializePrototypeIfRequired(Guid id)
        {
            switch (prototypeService.GetPrototypeType(id))
            {
                case PrototypeType.Permanent:  return;
                case PrototypeType.Temporary:
                {
                    try
                    {
                        var events = this.inMemoryEventStore.Read(id, 0).ToList();
                        this.prototypeService.RemovePrototype(id);

                        ThrowIfAssignmentQuantityLimitReached(events);
                        
                        var uncommittedEvents = events.Select(e => new UncommittedEvent(e.EventIdentifier, e.EventSourceId,
                            e.EventSequence, 0, e.EventTimeStamp, e.Payload));

                        var eventStream = new UncommittedEventStream("prototype", uncommittedEvents);
                        var committed = eventStore.Store(eventStream);
                        
                        this.liteEventBus.PublishCommittedEvents(committed);
                    }
                    catch
                    {
                        this.prototypeService.MarkAsPrototype(id, PrototypeType.Temporary);
                        throw;
                    }

                    this.cache.EvictAggregateRoot(id);
                    break;
                }
            }
        }

        private void ThrowIfAssignmentQuantityLimitReached(List<CommittedEvent> events)
        {
            events.First().
            this.cache.EvictAggregateRoot(id);
            throw new Exception("Assigment quantity limit reached");
        }
    }
}

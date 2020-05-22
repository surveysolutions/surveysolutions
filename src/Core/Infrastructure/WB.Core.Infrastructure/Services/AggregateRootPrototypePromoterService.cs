using System;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.Core.Infrastructure.Services
{
    class AggregateRootPrototypePromoterService : IAggregateRootPrototypePromoterService
    {
        private readonly ILiteEventBus liteEventBus;
        private readonly IInMemoryEventStore inMemoryEventStore;
        private readonly IEventStore eventStore;
        private readonly IAggregateRootPrototypeService prototypeService;
        private readonly IAggregateRootCacheCleaner cacheCleaner;

        public AggregateRootPrototypePromoterService(ILiteEventBus liteEventBus,
            IInMemoryEventStore inMemoryEventStore, IEventStore eventStore, 
            IAggregateRootPrototypeService prototypeService, IAggregateRootCacheCleaner cacheCleaner)
        {
            this.liteEventBus = liteEventBus;
            this.inMemoryEventStore = inMemoryEventStore;
            this.eventStore = eventStore;
            this.prototypeService = prototypeService;
            this.cacheCleaner = cacheCleaner;
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
                        var events = this.inMemoryEventStore.Read(id, 0);
                        this.prototypeService.RemovePrototype(id);
                        
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

                    this.cacheCleaner.Evict(id);
                    break;
                }
            }
        }
    }
}

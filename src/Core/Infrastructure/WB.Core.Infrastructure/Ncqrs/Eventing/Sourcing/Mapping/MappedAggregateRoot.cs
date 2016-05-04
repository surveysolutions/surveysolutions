using System;
using System.Linq;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    public abstract class MappedAggregateRoot : EventSourcedAggregateRoot
    {
        [NonSerialized] 
        private readonly IEventHandlerMappingStrategy _mappingStrategy;

        protected MappedAggregateRoot(IEventHandlerMappingStrategy strategy)
        {
            _mappingStrategy = strategy;
            InitializeHandlers();
        }

        protected MappedAggregateRoot(Guid id, IEventHandlerMappingStrategy strategy) : base(id)
        {
            _mappingStrategy = strategy;
            InitializeHandlers();
        }

        protected void InitializeHandlers()
        {
            foreach (var handler in _mappingStrategy.GetEventHandlers(this))
                RegisterHandler(handler);
        }

        public bool CanApplyHistory(CommittedEventStream history) => history.All(this.CanHandleEvent);

        protected override bool CanHandleEvent(CommittedEvent committedEvent)
            => this._mappingStrategy.CanHandleEvent(this, committedEvent.Payload.GetType());
    }
}
using System;
using System.Collections.Generic;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher
{
    public class UnpublishedEventContext : IDisposable
    {
        [ThreadStatic]
        private static UnpublishedEventContext _threadInstance;

        private readonly List<UncommittedEvent> _events = new List<UncommittedEvent>();
        private Action<AggregateRoot, UncommittedEvent> _eventAppliedCallback;

        public IEnumerable<UncommittedEvent> Events
        {
            get { return this._events; }
        }

        public static UnpublishedEventContext Current
        {
            get
            {
                return _threadInstance;
            }
        }

        public bool IsDisposed
        {
            get;
            private set;
        }

        private readonly NcqrCompatibleEventDispatcher eventDispatcher;

        public UnpublishedEventContext()
        {
            _threadInstance = this;
            this.IsDisposed = false;
            this.eventDispatcher = NcqrsEnvironment.Get<IEventBus>() as NcqrCompatibleEventDispatcher;
            this.InitializeAppliedEventHandler();
        }

        private void InitializeAppliedEventHandler()
        {
            if(this._eventAppliedCallback == null)
                this._eventAppliedCallback = new Action<AggregateRoot, UncommittedEvent>(this.AggregateRootEventAppliedHandler);

            AggregateRoot.RegisterThreadStaticEventAppliedCallback(this._eventAppliedCallback);
        }

        private void DestroyAppliedEventHandler()
        {
            if(this._eventAppliedCallback != null)
                AggregateRoot.UnregisterThreadStaticEventAppliedCallback(this._eventAppliedCallback);
        }

        private void AggregateRootEventAppliedHandler(AggregateRoot aggregateRoot, UncommittedEvent evnt)
        {
            this._events.Add(evnt);
            if (eventDispatcher != null)
            {
                eventDispatcher.IgnoreEventWithId(evnt.EventIdentifier);
            }
        }

        ~UnpublishedEventContext()
        {
             this.Dispose(false);
        }

        public void Dispose()
        {
             this.Dispose(true);
             GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.DestroyAppliedEventHandler();
                    _threadInstance = null;
                }

                this.IsDisposed = true;
            }
        }
    }
}

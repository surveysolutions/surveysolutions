using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using Ncqrs.Eventing;

namespace WB.Tests.Unit.Designer
{
    internal class EventContext : IDisposable
    {
        [ThreadStatic]
        private static EventContext _threadInstance;

        private readonly List<UncommittedEvent> _events = new List<UncommittedEvent>();
        private Action<EventSourcedAggregateRoot, UncommittedEvent> _eventAppliedCallback;

        public IEnumerable<UncommittedEvent> Events
        {
            get { return this._events; }
        }

        /// <summary>
        /// Gets the <see cref="EventContext"/> associated with the current thread context.
        /// </summary>
        /// <value>The current.</value>
        public static EventContext Current
        {
            get
            {
                return _threadInstance;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get;
            private set;
        }

        public EventContext()
        {
            _threadInstance = this;
            this.IsDisposed = false;

            this.InitializeAppliedEventHandler();
        }

        private void InitializeAppliedEventHandler()
        {
            if(this._eventAppliedCallback == null)
                this._eventAppliedCallback = new Action<EventSourcedAggregateRoot, UncommittedEvent>(this.AggregateRootEventAppliedHandler);

            EventSourcedAggregateRoot.RegisterThreadStaticEventAppliedCallback(this._eventAppliedCallback);
        }

        private void DestroyAppliedEventHandler()
        {
            if(this._eventAppliedCallback != null)
                EventSourcedAggregateRoot.UnregisterThreadStaticEventAppliedCallback(this._eventAppliedCallback);
        }

        private void AggregateRootEventAppliedHandler(EventSourcedAggregateRoot aggregateRoot, UncommittedEvent evnt)
        {
            this._events.Add(evnt);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// </summary>
        ~EventContext()
        {
             this.Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
             this.Dispose(true);
             GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

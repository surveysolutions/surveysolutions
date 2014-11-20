using System;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Logging;

namespace Ncqrs.Domain
{
    public abstract class UnitOfWorkBase : IUnitOfWorkContext
    {
        private readonly Guid _commandId;
        private readonly Action<AggregateRoot, UncommittedEvent> _eventAppliedCallback;

        /// <summary>
        /// Gets the id of command which triggered this unit of work.
        /// </summary>
        protected Guid CommandId
        {
            get { return _commandId; }
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="commandId">Id of command being processed.</param>
        protected UnitOfWorkBase(Guid commandId)
        {
            _commandId = commandId;
#warning Slava: restore Thread.CurrentThread.ManagedThreadId

            _eventAppliedCallback = new Action<AggregateRoot, UncommittedEvent>(AggregateRootEventAppliedHandler);
            IsDisposed = false;

            InitializeAppliedEventHandler();
        }

        private void InitializeAppliedEventHandler()
        {
            AggregateRoot.RegisterThreadStaticEventAppliedCallback(_eventAppliedCallback);
        }

        private void DestroyAppliedEventHandler()
        {
            AggregateRoot.UnregisterThreadStaticEventAppliedCallback(_eventAppliedCallback);
        }

        protected abstract void AggregateRootEventAppliedHandler(AggregateRoot aggregateRoot, UncommittedEvent evnt);

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="UnitOfWork"/> is reclaimed by garbage collection.
        /// </summary>
        ~UnitOfWorkBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DestroyAppliedEventHandler();
                    UnitOfWorkContext.Unbind();
                }

                IsDisposed = true;
            }
        }

        public abstract AggregateRoot GetById(Type aggregateRootType, Guid eventSourceId, long? lastKnownRevision);
        public TAggregateRoot GetById<TAggregateRoot>(Guid eventSourceId, long? lastKnownRevision) where TAggregateRoot : AggregateRoot
        {
            return (TAggregateRoot) GetById(typeof (TAggregateRoot), eventSourceId, lastKnownRevision);
        }

        public TAggregateRoot GetById<TAggregateRoot>(Guid eventSourceId) where TAggregateRoot : AggregateRoot
        {
            return GetById<TAggregateRoot>(eventSourceId, null);
        }

        public abstract void Accept();
    }
}
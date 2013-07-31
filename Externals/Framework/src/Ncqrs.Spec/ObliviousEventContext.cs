using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Domain;
using Ncqrs.Eventing;

namespace Ncqrs.Spec
{
    public class ObliviousEventContext : IDisposable
    {
        [ThreadStatic] 
        private static ObliviousEventContext _threadInstance;

        private Action<AggregateRoot, UncommittedEvent> _eventAppliedCallback;


        /// <summary>
        /// Gets the <see cref="EventContext"/> associated with the current thread context.
        /// </summary>
        /// <value>The current.</value>
        public static ObliviousEventContext Current
        {
            get { return _threadInstance; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed { get; private set; }

        public ObliviousEventContext()
        {
            _threadInstance = this;
            IsDisposed = false;

            InitializeAppliedEventHandler();
        }

        private void InitializeAppliedEventHandler()
        {
            if (_eventAppliedCallback == null)
                _eventAppliedCallback = new Action<AggregateRoot, UncommittedEvent>(AggregateRootEventAppliedHandler);

            AggregateRoot.RegisterThreadStaticEventAppliedCallback(_eventAppliedCallback);
        }

        private void DestroyAppliedEventHandler()
        {
            if (_eventAppliedCallback != null)
                AggregateRoot.UnregisterThreadStaticEventAppliedCallback(_eventAppliedCallback);
        }

        private void AggregateRootEventAppliedHandler(AggregateRoot aggregateRoot, UncommittedEvent evnt)
        {

        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="UnitOfWork"/> is reclaimed by garbage collection.
        /// </summary>
        ~ObliviousEventContext()
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
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DestroyAppliedEventHandler();
                    _threadInstance = null;
                }

                IsDisposed = true;
            }
        }
    }
}

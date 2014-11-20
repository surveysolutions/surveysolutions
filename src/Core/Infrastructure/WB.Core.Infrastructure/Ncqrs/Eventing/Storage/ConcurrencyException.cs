using System;

namespace Ncqrs.Eventing.Storage
{
    public class ConcurrencyException : Exception
    {
        private readonly Guid _eventSourceId;
        private readonly long _eventSourceVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="eventSourceId">The id of the event source.</param>
        /// <param name="versionToBeSaved">Version to be saved.</param>
        public ConcurrencyException(Guid eventSourceId, long versionToBeSaved)
            : this(eventSourceId, versionToBeSaved, null)
        {
        }

        public ConcurrencyException(Guid eventSourceId, long eventSourceVersion, Exception innerException)
            : base(
                String.Format(
                    "There is a newer than {0} version of the event source with id {1} you are trying to save stored in the event store.",
                    eventSourceVersion, eventSourceId), innerException)
        {
            this._eventSourceId = eventSourceId;
            this._eventSourceVersion = eventSourceVersion;
        }

        /// <summary>
        /// Gets the id of the event source.
        /// </summary>
        /// <value>The id event source.</value>
        public Guid EventSourceId
        {
            get { return _eventSourceId; }
        }


        /// <summary>
        /// Gets the event source version.
        /// </summary>
        /// <value>The event source version.</value>
        public long EventSourceVersion
        {
            get { return _eventSourceVersion; }
        }

        
    }
}

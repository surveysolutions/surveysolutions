// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllEventsStreamProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The all events stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncSreamProvider
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;

    using Ncqrs;
    using Ncqrs.Eventing.Storage.RavenDB;

    /// <summary>
    /// The all events stream provider.
    /// </summary>
    public class AllEventsStreamProvider : ISyncEventStreamProvider
    {
        #region Fields

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IStreamableEventStore eventStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AllEventsStreamProvider"/> class.
        /// </summary>
        public AllEventsStreamProvider()
        {
            this.eventStore = NcqrsEnvironment.Get<IStreamableEventStore>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get event stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<AggregateRootEvent> GetEventStream()
        {
            return this.eventStore.GetEventStream().Select(item => new AggregateRootEvent(item));
        }

        #endregion
    }
}
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

    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;
    

    /// <summary>
    /// The all events stream provider.
    /// </summary>
    public class AllIntEventsStreamProvider : IIntSyncEventStreamProvider
    {
        #region Fields

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IStreamableEventStore eventStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AllIntEventsStreamProvider"/> class.
        /// </summary>
        public AllIntEventsStreamProvider()
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

        /// <summary>
        /// The get total event count.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        public int? GetTotalEventCount()
        {
            return null;
        }

        public SynchronizationType SyncType
        {
            get
            {
                return SynchronizationType.Push;
            }
        }

        public string ProviderName
        {
            get
            {
                return "Backup";
            }
        }

        #endregion
    }
}
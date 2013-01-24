// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntClientEventStreamProvider.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The int client event stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncSreamProvider
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;
    

    /// <summary>
    /// The int client event stream provider.
    /// </summary>
    public class IntClientEventStreamProvider : IIntSyncEventStreamProvider
    {
        #region Fields

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IStreamableEventStore eventStore;

        /// <summary>
        /// The storage.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IntClientEventStreamProvider"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        public IntClientEventStreamProvider(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage)
        {
            this.eventStore = NcqrsEnvironment.Get<IStreamableEventStore>();
            this.storage = storage;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the provider name.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return "Client stream";
            }
        }

        /// <summary>
        /// Gets the sync type.
        /// </summary>
        public SynchronizationType SyncType
        {
            get
            {
                return SynchronizationType.Push;
            }
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
            return
                this.storage.Query().Where(item => SurveyStatus.IsStatusAllowCapiSync(item.Status)).SelectMany(
                    item =>
                    this.eventStore.ReadFrom(item.CompleteQuestionnaireId, int.MinValue, int.MaxValue).Select(
                        e => new AggregateRootEvent(e)));
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

        #endregion
    }
}
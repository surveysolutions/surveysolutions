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

    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs;
    using Ncqrs.Eventing.Storage.RavenDB;

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
            foreach (
                CompleteQuestionnaireBrowseItem item in
                    this.storage.Query().Where(item => SurveyStatus.IsStatusAllowCapiSync(item.Status)))
            {
                foreach (
                    AggregateRootEvent aggregateRootEvent in
                        this.eventStore.ReadFrom(item.CompleteQuestionnaireId, int.MinValue, int.MaxValue).Select(
                            e => new AggregateRootEvent(e)))
                {
                    yield return aggregateRootEvent;
                }
            }
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
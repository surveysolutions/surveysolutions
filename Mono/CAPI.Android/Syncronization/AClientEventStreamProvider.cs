// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AClientEventStreamProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The int client event stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.DenormalizerStorage;
using Main.Synchronization.SyncSreamProvider;
using Ncqrs;
using Ncqrs.Eventing.Storage;

namespace CAPI.Android.Syncronization
{
    /// <summary>
    /// The a client event stream provider.
    /// </summary>
    public class AClientEventStreamProvider : IIntSyncEventStreamProvider
    {
        #region Fields

        /// <summary>
        /// The document storage.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireView> documentStorage;

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IStreamableEventStore eventStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AClientEventStreamProvider"/> class.
        /// </summary>
        /// <param name="documentStorage">
        /// The document storage.
        /// </param>
        public AClientEventStreamProvider(IDenormalizerStorage<CompleteQuestionnaireView> documentStorage)
        {
            this.eventStore = NcqrsEnvironment.Get<IStreamableEventStore>();
            this.documentStorage = documentStorage;
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
                return "Client stream.";
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
                this.documentStorage.Query().Where(item => SurveyStatus.IsStatusAllowCapiSync(item.Status)).SelectMany(
                    item =>
                    this.eventStore.ReadFrom(item.PublicKey, int.MinValue, int.MaxValue).Select(
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
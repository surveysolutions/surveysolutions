// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AClientEventStreamProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The int client event stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.DenormalizerStorage;
using Main.Synchronization.SyncSreamProvider;
using Ncqrs;
using Ncqrs.Eventing.Storage;

namespace CAPI.Android.Core.Model.Syncronization
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
        private readonly IDenormalizerStorage<QuestionnaireDTO> documentStorage;
        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IStreamableEventStore eventStore;

        #endregion

        #region Constructors and Destructors

        public AClientEventStreamProvider(IDenormalizerStorage<QuestionnaireDTO> documentStorage)
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
        /*    return
                this.documentStorage.Query().Where(item => SurveyStatus.IsStatusAllowCapiSync(item.Status)).SelectMany(
                    item =>
                    this.eventStore.ReadFrom(item.PublicKey, int.MinValue, int.MaxValue).Select(
                        e => new AggregateRootEvent(e)));*/
            var allovedStatuses = SurveyStatus.GetListOfAllowerdStatusesForSync().Select(s => s.ToString()).ToArray();
            var docs = documentStorage.Query(item =>allovedStatuses.Contains(item.Status)).ToList();
            return docs
                  .SelectMany(
                      item =>
                      this.eventStore.ReadFrom(Guid.Parse(item.Id), int.MinValue, int.MaxValue).Select(
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
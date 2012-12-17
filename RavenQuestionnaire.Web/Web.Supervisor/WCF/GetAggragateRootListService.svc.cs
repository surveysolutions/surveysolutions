// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAggragateRootListService.svc.cs" company="The World Bank">
//   Get Aggragate Root List Service
// </copyright>
// <summary>
//   The get aggragate root list service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.CompleteQuestionnaire;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Questionnaire.Core.Web.Helpers;

    using SynchronizationMessages.CompleteQuestionnaire;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetAggragateRootListService" in code, svc and config file together.

    /// <summary>
    /// The get aggragate root list service.
    /// </summary>
    public class GetAggragateRootListService : IGetAggragateRootList
    {
        #region Constants and Fields

        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventStreamReader eventStore;

        /// <summary>
        /// The syncs process factory
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAggragateRootListService"/> class.
        /// </summary>
        /// <param name="eventStore">
        /// The event store.
        /// </param>
        /// <param name="syncProcessFactory">
        /// The sync Process Factory.
        /// </param>
        public GetAggragateRootListService(IEventStreamReader eventStore, ISyncProcessFactory syncProcessFactory)
        {
            this.eventStore = eventStore;
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <returns>
        /// List of AR events
        /// </returns>
        public ListOfAggregateRootsForImportMessage Process()
        {
            Guid syncProcess = Guid.NewGuid();
            try
            {
                var process = (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                return process.Export("Supervisor export AR events");
            }
            catch (Exception ex)
            {
                return new ListOfAggregateRootsForImportMessage();
            }
        }

        #endregion
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetEventStreamService.svc.cs" company="The World Bank">
//   Get Event Stream Service
// </copyright>
// <summary>
//   The get event stream service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Events;

    using Questionnaire.Core.Web.Helpers;

    using SynchronizationMessages.CompleteQuestionnaire;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetEventStreamService" in code, svc and config file together.

    /// <summary>
    /// The get event stream service.
    /// </summary>
    public class GetEventStreamService : IGetEventStream
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
        /// Initializes a new instance of the <see cref="GetEventStreamService"/> class.
        /// </summary>
        /// <param name="eventStore">
        /// The event store.
        /// </param>
        /// <param name="syncProcessFactory">
        /// The sync Process Factory.
        /// </param>
        public GetEventStreamService(IEventStreamReader eventStore, ISyncProcessFactory syncProcessFactory)
        {
            this.eventStore = eventStore;
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="firstEventPulicKey">
        /// The first event pulic key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// List of AR events
        /// </returns>
        public ImportSynchronizationMessage Process(Guid firstEventPulicKey, int length)
        {
            Guid syncProcess = Guid.NewGuid();
            try
            {
                throw new NotImplementedException("proper synchronization with login password request is not implemented");
                var process = this.syncProcessFactory.GetRestProcess(syncProcess, Guid.NewGuid());

                return process.Export("Supervisor export AR events", firstEventPulicKey, length);
            }
            catch (Exception ex)
            {
                return new ImportSynchronizationMessage();
            }
        }

        #endregion
    }
}
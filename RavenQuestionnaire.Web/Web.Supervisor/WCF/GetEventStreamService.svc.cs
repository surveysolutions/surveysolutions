// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetEventStreamService.svc.cs" company="The World Bank">
//   
// </copyright>
// <summary>
//   The get event stream service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System;
    using System.Linq;

    using DataEntryClient.CompleteQuestionnaire;

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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEventStreamService"/> class.
        /// </summary>
        /// <param name="eventStore">
        /// The event store.
        /// </param>
        public GetEventStreamService(IEventStreamReader eventStore)
        {
            this.eventStore = eventStore;
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
                var process = new EventSyncProcess(KernelLocator.Kernel, syncProcess);

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
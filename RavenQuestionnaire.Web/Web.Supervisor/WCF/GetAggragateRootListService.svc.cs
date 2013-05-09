// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAggragateRootListService.svc.cs" company="">
//   
// </copyright>
// <summary>
//   The get aggragate root list service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.WCF
{
    using System;
    using System.IO;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Web;

    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Events;

    using SynchronizationMessages.CompleteQuestionnaire;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetAggragateRootListService" in code, svc and config file together.

    /// <summary>
    /// The get aggragate root list service.
    /// </summary>
    public class GetAggragateRootListService : IGetAggragateRootList
    {
        #region Fields

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
                throw new NotImplementedException("proper synchronization with login password request is not implemented");
                var process = this.syncProcessFactory.GetRestProcess(syncProcess, Guid.NewGuid());

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
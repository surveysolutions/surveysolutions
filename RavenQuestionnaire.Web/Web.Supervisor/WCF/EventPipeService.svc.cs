// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventPipeService.svc.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The event pipe service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.WCF
{
    using System;
    using System.ServiceModel.Activation;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Events;

    using Ninject;

    using Questionnaire.Core.Web.Helpers;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// The event pipe service.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EventPipeService : IEventPipe
    {
        #region Constants and Fields

        /// <summary>
        /// The kernel.
        /// </summary>
        private readonly IKernel kernel;

        /// <summary>
        /// The syncs process factory
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPipeService"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="syncProcessFactory">
        /// The syncs process factory
        /// </param>
        public EventPipeService(IKernel kernel, ISyncProcessFactory syncProcessFactory)
        {
            this.kernel = kernel;
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// Error code
        /// </returns>
        public ErrorCodes Process(EventSyncMessage request)
        {
            Guid syncProcess = Guid.NewGuid();
            try
            {
                throw new NotImplementedException("proper synchronization with login password request is not implemented");
                var process = this.syncProcessFactory.GetRestProcess(syncProcess, Guid.NewGuid());

                process.Import("WCF syncronization", request.Command);

                return ErrorCodes.None;
            }
            catch (Exception ex)
            {
                return ErrorCodes.Fail;
            }
        }

        #endregion
    }
}
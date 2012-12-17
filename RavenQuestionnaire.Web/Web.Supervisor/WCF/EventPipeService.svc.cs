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

    using DataEntryClient.CompleteQuestionnaire;

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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPipeService"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public EventPipeService(IKernel kernel)
        {
            this.kernel = kernel;
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
                var process = new EventSyncProcess(KernelLocator.Kernel, syncProcess, request.SynchronizationKey);

                process.Import("WCF syncronization", request.Command);

                return ErrorCodes.None;
            }
            catch (Exception)
            {
                return ErrorCodes.Fail;
            }
        }

        #endregion
    }
}
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

    using Main.Core.Events;

    using Ninject;

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
            try
            {
                // kernel.Get<ICommandInvoker>().Execute(request.Command, request.CommandKey, request.SynchronizationKey);
                var eventStore = this.kernel.Get<IEventSync>();
                eventStore.WriteEvents(request.Command);
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
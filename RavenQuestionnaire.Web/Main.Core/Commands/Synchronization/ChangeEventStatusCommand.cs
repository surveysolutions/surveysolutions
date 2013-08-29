namespace Main.Core.Commands.Synchronization
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The change event status command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "ChangeAggregateRootStatus")]
    public class ChangeEventStatusCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeEventStatusCommand"/> class.
        /// </summary>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="eventChunckPublicKey">
        /// The event chunck public key.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        public ChangeEventStatusCommand(Guid processGuid, Guid eventChunckPublicKey, EventState status)
        {
            this.EventChunckPublicKey = eventChunckPublicKey;
            this.Status = status;
            this.ProcessGuid = processGuid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the event chunck public key.
        /// </summary>
        public Guid EventChunckPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public EventState Status { get; set; }

        #endregion
    }
}
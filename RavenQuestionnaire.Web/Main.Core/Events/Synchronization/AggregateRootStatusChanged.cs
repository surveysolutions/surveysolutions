namespace Main.Core.Events.Synchronization
{
    using System;

    using Main.Core.Documents;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The aggregate root status changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AggregateRootStatusChanged")]
    public class AggregateRootStatusChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the event chunck public key.
        /// </summary>
        public Guid EventChunckPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public EventState Status { get; set; }

        #endregion
    }
}
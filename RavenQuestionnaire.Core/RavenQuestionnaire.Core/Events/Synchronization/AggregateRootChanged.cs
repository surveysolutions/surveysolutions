// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregateRootChanged.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The aggregate root status changed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events.Synchronization
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Documents;

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
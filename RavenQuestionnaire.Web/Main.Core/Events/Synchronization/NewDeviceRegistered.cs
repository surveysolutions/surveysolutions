// -----------------------------------------------------------------------
// <copyright file="NewDeviceRegistered.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Events.Synchronization
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new device registered
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewDeviceRegistered")]
    public class NewDeviceRegistered
    {
        #region PublicProperties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime RegisteredDate { get; set; }

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid TabletId { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Registrator.
        /// </summary>
        public Guid Registrator { get; set; }

        #endregion
    }
}
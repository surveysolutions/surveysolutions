using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Synchronization
{
    /// <summary>
    /// The new device registered
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewDeviceRegistered")]
    public class NewDeviceRegistered
    {
        #region PublicProperties

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid IdForRegistration { get; set; }

        /// <summary>
        /// Gets or sets Registrator.
        /// </summary>
        public Guid Registrator { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime RegisteredDate { get; set; }

        #endregion
    }
}
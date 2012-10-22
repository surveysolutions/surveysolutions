// -----------------------------------------------------------------------
// <copyright file="RegisterNewDeviceCommand.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Commands.Synchronization
{
    using System;
    using Main.Core.Domain;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(DeviceAR))]
    public class RegisterNewDeviceCommand : CommandBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets RegisterGuid.
        /// </summary>
        [AggregateRootId]
        public Guid RegisterGuid { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets RegisteredDate.
        /// </summary>
        public DateTime RegisteredDate { get; set; }

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid TabletId { get; set; }

        #endregion
    }
}

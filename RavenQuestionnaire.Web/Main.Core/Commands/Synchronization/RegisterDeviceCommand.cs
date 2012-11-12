// -----------------------------------------------------------------------
// <copyright file="RegisterDeviceCommand.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Commands.Synchronization
{
    using System;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(DeviceAR))]
    public class RegisterDeviceCommand : CommandBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterDeviceCommand"/> class.
        /// </summary>
        /// <param name="description">
        ///   The description.
        /// </param>
        /// <param name="secretKey">
        ///   The secret key.
        /// </param>
        /// <param name="tabletId">
        ///   The tablet id.
        /// </param>
        /// <param name="currentUser"></param>
        public RegisterDeviceCommand(string description, byte[] secretKey, Guid tabletId, UserLight supervisor)
        {
            this.RegisterGuid = tabletId;
            this.Description = description;
            this.SecretKey = secretKey;
            this.TabletId = tabletId;
            this.RegisteredDate = DateTime.Today;
            this.Supervisor = supervisor;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Supervisor.
        /// </summary>
        public UserLight Supervisor { get; set; }

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

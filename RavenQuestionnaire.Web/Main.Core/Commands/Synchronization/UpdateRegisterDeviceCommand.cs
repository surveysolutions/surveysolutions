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
    [MapsToAggregateRootMethod(typeof(DeviceAR), "UpdateDevice")]
    public class UpdateRegisterDeviceCommand : CommandBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRegisterDeviceCommand"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="secretKey">
        /// The secret key.
        /// </param>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        /// <param name="guidSupervisor">
        /// The guid Supervisor.
        /// </param>
        public UpdateRegisterDeviceCommand(string description, Guid publicKey, byte[] secretKey, Guid registrator)
        {
            this.PublicKey = publicKey;
            this.Registrator = registrator;
            this.Description = description;
            this.SecretKey = secretKey;
        }

        #endregion

        #region Public Properties

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets registrator.
        /// </summary>
        public Guid Registrator { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        #endregion

    }
}

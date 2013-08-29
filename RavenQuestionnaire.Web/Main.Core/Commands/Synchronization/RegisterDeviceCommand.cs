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
    public class RegisterDeviceCommand : CommandBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterDeviceCommand"/> class.
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
        public RegisterDeviceCommand(string description, Guid publicKey, byte[] secretKey, Guid idForRegistration, Guid registrator)
        {
            this.PublicKey =  publicKey;
            this.Registrator = registrator;
            this.IdForRegistration = idForRegistration;
            this.Description = description;
            this.SecretKey = secretKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets public Key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid IdForRegistration { get; set; }

        /// <summary>
        /// Gets or sets Supervisor.
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

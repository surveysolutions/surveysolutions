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
    public class RegisterNewDeviceCapiCommand : CommandBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterNewDeviceCapiCommand"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="publicKeySupervisor">
        /// The public Key Supervisor.
        /// </param>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        public RegisterNewDeviceCapiCommand(string description, byte[] publicKeySupervisor, Guid tabletId)
        {
            this.Description = description;
            this.SupervisorPublicKey = publicKeySupervisor;
            this.TabletId = tabletId;
            this.RegisteredDate = DateTime.Today;
        }

        #endregion

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
        public byte[] SupervisorPublicKey { get; set; }

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

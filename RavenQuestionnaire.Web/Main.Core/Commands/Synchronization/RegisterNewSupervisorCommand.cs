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
    public class RegisterNewSupervisorCommand : CommandBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterNewSupervisorCommand"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="publicKeyCapi">
        /// The public key capi.
        /// </param>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        public RegisterNewSupervisorCommand(string description, byte[] publicKeyCapi, Guid tabletId)
        {
            this.PublicKeyCapi = publicKeyCapi;
            this.Description = description;
            this.TabletId = tabletId;
            this.RegisteredDate = DateTime.Now;
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
        public byte[] PublicKeyCapi { get; set; }

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

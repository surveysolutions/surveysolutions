namespace Main.Core.Commands.Synchronization
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The create new synchronization process command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(SyncProcessAR))]
    public class CreateNewSynchronizationProcessCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateNewSynchronizationProcessCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        ///   The public key.
        /// </param>
        /// <param name="parentProcessKey">
        /// The parent process key
        /// </param>
        /// <param name="synckType">
        ///   The sync type.
        /// </param>
        /// <param name="description">
        ///   The description
        /// </param>
        public CreateNewSynchronizationProcessCommand(Guid publicKey, Guid? parentProcessKey, SynchronizationType synckType, string description)
        {
            this.PublicKey = publicKey;
            this.ParentProcessKey = parentProcessKey;
            this.SynckType = synckType;
            this.Description = description;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets ParentProcessKey.
        /// </summary>
        public Guid? ParentProcessKey { get; set; }

        /// <summary>
        /// Gets or sets the sync type.
        /// </summary>
        public SynchronizationType SynckType { get; set; }

        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }
}
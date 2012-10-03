// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateNewSynchronizationProcessCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The create new synchronization process command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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
        /// The public key.
        /// </param>
        /// <param name="synckType">
        /// The synck type.
        /// </param>
        public CreateNewSynchronizationProcessCommand(Guid publicKey, SynchronizationType synckType)
        {
            this.PublicKey = publicKey;
            this.SynckType = synckType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the synck type.
        /// </summary>
        public SynchronizationType SynckType { get; set; }

        #endregion
    }
}
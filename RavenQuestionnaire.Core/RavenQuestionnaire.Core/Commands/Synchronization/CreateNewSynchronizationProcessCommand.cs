// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateNewSynchronizationProcessCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The create new synchronization process command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Domain;

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
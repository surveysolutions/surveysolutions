namespace Main.Core.Commands.File
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The delete file command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(FileAR), "DeleteFile")]
    public class DeleteFileCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteFileCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public DeleteFileCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the public key.
        /// </summary>
        [AggregateRootId]
        public Guid PublicKey { get; private set; }

        #endregion
    }
}
namespace Main.Core.Commands.File
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The update file meta command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(FileAR), "UpdateFileMeta")]
    public class UpdateFileMetaCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFileMetaCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="desc">
        /// The desc.
        /// </param>
        public UpdateFileMetaCommand(Guid publicKey, string title, string desc)
        {
            this.PublicKey = publicKey;
            this.Description = desc;
            this.Title = title;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; private set; }

        #endregion
    }
}
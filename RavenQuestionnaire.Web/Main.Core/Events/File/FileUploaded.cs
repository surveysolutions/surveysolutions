namespace Main.Core.Events.File
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The file uploaded.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:FileUploaded")]
    public class FileUploaded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the original file.
        /// </summary>
        public string OriginalFile { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}
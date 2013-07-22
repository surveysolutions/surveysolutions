namespace Main.Core.Domain
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Events.File;

    using Ncqrs.Domain;

    /// <summary>
    /// The file ar.
    /// </summary>
    public class FileAR : AggregateRootMappedByConvention
    {
        #region Fields

        //private FileDescription innerDocument = new FileDescription();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAR"/> class.
        /// </summary>
        public FileAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAR"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="originalFile">
        /// The original file.
        /// </param>
        public FileAR(Guid publicKey, string title, string description, string originalFile)
            : base(publicKey)
        {
            this.ApplyEvent(
                new FileUploaded
                    {
                       PublicKey = publicKey, Title = title, Description = description, OriginalFile = originalFile 
                    });
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The delete file.
        /// </summary>
        public void DeleteFile()
        {
            this.ApplyEvent(new FileDeleted { PublicKey = this.EventSourceId });
        }

        /// <summary>
        /// The on file deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public void OnFileDeleted(FileDeleted e)
        {
            //this.innerDocument = null;

            // storage.DeleteFile(e.PublicKey.ToString());
            // storage.DeleteFile(string.Format(thumbFormat, e.PublicKey));
        }

        /// <summary>
        /// The update file meta.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public void UpdateFileMeta(string title, string description)
        {
            this.ApplyEvent(
                new FileMetaUpdated { PublicKey = this.EventSourceId, Description = description, Title = title });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on file meta updated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnFileMetaUpdated(FileMetaUpdated e)
        {
        }

        /// <summary>
        /// The on file uploaded.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnFileUploaded(FileUploaded e)
        {
            /*this.innerDocument = new FileDescription
                {
                   FileName = e.PublicKey.ToString(), 
                   Description = e.Description, 
                   Title = e.Title 
                };*/

            //// storage.StoreFile(originalFile);

            /*    using (var thumb = FromBase64(e.ThumbFile))
                {
                    var tumbFile = new FileDescription()
                    {
                        PublicKey =string.Format(thumbFormat,e.PublicKey),
                        Content = thumb,
                        Title = string.Empty,
                        Description = string.Empty
                    };
                    storage.StoreFile(tumbFile);
                  //  attachments.Store(tumbFile, evnt.Payload.ImagePublicKey);
                }*/
        }

        #endregion
    }
}
namespace Main.Core.Commands.File
{
    using System;
    using System.IO;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The upload file command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(FileAR))]
    public class UploadFileCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileCommand"/> class.
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
        /// <param name="origData">
        /// The orig data.
        /// </param>
        public UploadFileCommand(Guid publicKey, string title, string desc, Stream origData)
            : this(publicKey, title, desc)
        {
            this.OriginalFile = this.ToBase64(origData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileCommand"/> class.
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
        /// <param name="origData">
        /// The orig data.
        /// </param>
        public UploadFileCommand(Guid publicKey, string title, string desc, string origData)
            : this(publicKey, title, desc)
        {
            this.OriginalFile = origData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileCommand"/> class.
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
        protected UploadFileCommand(Guid publicKey, string title, string desc)
        {
            this.PublicKey = publicKey;
            this.Description = desc;
            this.Title = title;
        }

        #endregion

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

        #region Methods

        /// <summary>
        /// The to base 64.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        protected string ToBase64(Stream stream)
        {
            string base64;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                base64 = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }

            return base64;
        }

        #endregion
    }
}
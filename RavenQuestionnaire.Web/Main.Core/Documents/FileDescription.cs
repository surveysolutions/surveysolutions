// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDescription.cs" company="">
//   
// </copyright>
// <summary>
//   The file description.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Documents
{
    using System;
    using System.IO;

    /// <summary>
    /// The file description.
    /// </summary>
    public class FileDescription
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public Stream Content { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public string FileName { get; set; }

        // public Guid ThumbPublicKey { get; set; }
        // public DateTime CreationDate { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}
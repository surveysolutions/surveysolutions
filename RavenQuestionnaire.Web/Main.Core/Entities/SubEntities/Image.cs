// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Image.cs" company="">
//   
// </copyright>
// <summary>
//   The image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The image.
    /// </summary>
    public class Image
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion

        /*   public string OriginalBase64 { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string ThumbnailBase { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }*/
    }
}
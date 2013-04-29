// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageUploaded.cs" company="">
//   
// </copyright>
// <summary>
//   The image uploaded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The image uploaded.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:ImageUploaded")]
    public class ImageUploaded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image public key.
        /// </summary>
        public Guid ImagePublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion

        // public string FileName { get; set; }

        // public string ThumbName { get; set; }
    }
}
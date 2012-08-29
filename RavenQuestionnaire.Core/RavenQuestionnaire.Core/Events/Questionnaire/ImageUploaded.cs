// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageUploaded.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The image uploaded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The image uploaded.
    /// </summary>
    [Serializable]
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